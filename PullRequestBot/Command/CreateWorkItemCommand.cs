using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Octokit;
using PullRequestLibrary.Generated.GitHub.PRCommentCreated;
using PullRequestLibrary.Provider.AzureDevOps;
using PullRequestLibrary.Provider.GitHub;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.WindowsAzure.Storage.Table;
using PullRequestLibrary.Model;
using PullRequest = Octokit.PullRequest;

namespace PullRequestBot.Command
{
    public class CreateWorkItemCommand
    {
        private readonly IGitHubRepository _gitHubRepository;
        private readonly IWorkItemRepository _workItemRepository;

        public CreateWorkItemCommand(IGitHubRepository gitHubRepository, IWorkItemRepository workItemRepository)
        {
            this._gitHubRepository = gitHubRepository;
            _workItemRepository = workItemRepository;
        }

        [FunctionName("CreateWorkItemCommand")]
        public async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Get Parent Info 
            // Ask the entity or StorageAccount if there is duplication
            // Create a work item

            var comment = context.GetInput<PRCommentCreated>();

            // Get the parent review comment
            var parentReviewComment = await context.CallActivityAsync<PullRequestReviewComment>("CreateWorkItemCommand_GetParentReview", comment);
            // Get the State of the PullRequestState
            var pullRequestState =
                await context.CallActivityAsync<PullRequestState>("CreateWorkItemCommand_GetPullRequestState", comment);
            
            // Ask the entity has duplication  
            string entityId = pullRequestState?.EntityId ?? context.NewGuid().ToString();

            EntityId id = new EntityId();

            var pullRequestDetailContext = await context.CallEntityAsync<PullRequestStateContext>(new EntityId(nameof(PullRequestStateContext), entityId), "get", null);

            // If you don't start CI however, already have a work item comment. (maybe it is rare case)
            pullRequestDetailContext = pullRequestDetailContext ?? new PullRequestStateContext();

            // create a work item
            if (!pullRequestDetailContext.HasCreatedWorkItem(comment.comment.id))
            {
                WorkItem createdWorkItem =
                    await context.CallActivityAsync<WorkItem>("CreateWorkItemCommand_CreateWorkItem",
                        parentReviewComment);
                pullRequestDetailContext.Add(
                    new CreatedWorkItem() {
                        CommentId = comment.comment.id
                        });
            }
             
            // update Entity 

            await context.CallEntityAsync<PullRequestStateContext>(
                new EntityId(nameof(PullRequestStateContext), entityId), "update", pullRequestDetailContext);

            // update Status.

            pullRequestState = pullRequestState ?? new PullRequestState();
            pullRequestState.EntityId = entityId;
            pullRequestState.PartitionKey = pullRequestState.PartitionKey ?? comment.repository.full_name;
            pullRequestState.RowKey = pullRequestState.RowKey ?? comment.pull_request.id.ToString(); 

            await context.CallActivityAsync("CreateWorkItemCommand_CreateOrUpdatePullRequestState", pullRequestState);

            return outputs;
        }

        [FunctionName("CreateWorkItemCommand_GetParentReview")]
        public async Task<PullRequestReviewComment> GetParentReviewAsync([ActivityTrigger] PRCommentCreated comment, ILogger log)
        {
            var comments = await _gitHubRepository.GetPullRequestReviewComments(comment.pull_request.id);
            return comments
                .FirstOrDefault(x => ((x.PullRequestReviewId == comment.comment.pull_request_review_id) && (x.InReplyToId is null)));
        }

        [FunctionName("CreateWorkItemCommand_GetPullRequestState")]
        public async Task<PullRequestState> GetPullRequestState(
            [ActivityTrigger] PRCommentCreated comment, 
            [Table("PullRequestState")] CloudTable cloudTable,
            ILogger log)
        {
            TableQuery<PullRequestState> query = new TableQuery<PullRequestState>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, comment.repository.full_name),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,  comment.pull_request.id.ToString())));
            var pullRequetStates = await cloudTable.ExecuteQuerySegmentedAsync(query, null);
            return pullRequetStates.Results.FirstOrDefault(); // Since we specify one record, it should be only one or null.
        }

        [FunctionName("CreateWorkItemCommand_CreateOrUpdatePullRequestState")]
        public async Task CreateOrUpdatePullRequestState(
            [ActivityTrigger] PullRequestState state,
            [Table("PullRequestState")] IAsyncCollector<PullRequestState> collector,
            ILogger log)
        {

            await collector.AddAsync(state);
        }

        [FunctionName("CreateWorkItemCommand_CreateWorkItem")]
        public async Task<WorkItem> CreateWorkItem(
            [ActivityTrigger] PullRequestReviewComment parentReviewComment,
            ILogger log
        )
        {
            var workItem = new WorkItemSource()
            {
                Title = $"SonarCloud Issue [{parentReviewComment.PullRequestReviewId}][{parentReviewComment.Id}]",
                Description = parentReviewComment.Body
            };
            WorkItem createdWorkItem = await _workItemRepository.CreateWorkItem(workItem);
            return createdWorkItem;
        }

        [FunctionName("PullRequestEntity")]
        public async Task<PullRequestStateContext> PullReuestEntity(
            [EntityTrigger] IDurableEntityContext ctx)
        {
            var current = ctx.GetState<PullRequestStateContext>();
            var input = ctx.GetInput<PullRequestStateContext>();

            switch (ctx.OperationName)
            {
                case "get":
                    break;
                case "update":
                    current = input;
                    break;
            }
            ctx.SetState(current);
            return current;
        }


    }
}