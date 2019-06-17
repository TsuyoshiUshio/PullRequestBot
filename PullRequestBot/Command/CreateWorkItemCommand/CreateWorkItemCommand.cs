using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PullRequestLibrary.Generated.GitHub.PRCommentCreated;
using PullRequestLibrary.Provider.AzureDevOps;
using PullRequestLibrary.Provider.GitHub;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Newtonsoft.Json.Linq;
using PullRequestBot.Command.PullRequestStateUtility;
using PullRequestBot.Entity;
using PullRequestLibrary.Model;
using PullRequest = Octokit.PullRequest;

namespace PullRequestBot.Command.CreateWorkItemCommand
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

        [FunctionName(nameof(CreateWorkItemCommand))]
        public async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Get Parent Info 
            // Ask the entity or StorageAccount if there is duplication
            // Create a work item

            var comment = context.GetInput<PRCommentCreated>();

            // Get the parent review comment
            // PullRequestReviewComment can't deserialize. 
            var parentReviewComment = await context.CallActivityAsync<JObject>(nameof(CreateWorkItemCommand) +"_GetParentReview", comment);
            // Get the State of the PullRequestState
            var pullRequestState =
                await context.CallActivityAsync<PullRequestState>("PullRequestStateUtility_GetPullRequestState", new PullRequestStateKey {
                    PartitionKey = comment.repository.full_name.ToPartitionKey(),
                    RowKey = comment.pull_request.id.ToString()});
            
            // Ask the entity has duplication  
            string entityId = pullRequestState?.EntityId ?? context.NewGuid().ToString();
            EntityStateResponse<PullRequestStateContext> response =
                await context.CallActivityAsync<EntityStateResponse<PullRequestStateContext>>(
                    nameof(CreateWorkItemCommand) + "_GetPullRequestStateContext", entityId);

            var pullRequestDetailContext = response.EntityState;
            // If you don't start CI however, already have a work item comment. (maybe it is rare case)
            pullRequestDetailContext = pullRequestDetailContext ?? new PullRequestStateContext();

            // create a work item
            pullRequestDetailContext = await ExecuteAsync(context, pullRequestDetailContext, comment, parentReviewComment);
             
            // update Entity 

            await context.CallEntityAsync<PullRequestStateContext>(
                new EntityId(nameof(PullRequestEntity), entityId), "update", pullRequestDetailContext);

            // update Status.

            pullRequestState = pullRequestState ?? new PullRequestState();
            pullRequestState.EntityId = entityId;
            pullRequestState.PartitionKey = pullRequestState.PartitionKey ?? comment.repository.full_name.ToPartitionKey();
            pullRequestState.RowKey = pullRequestState.RowKey ?? comment.pull_request.number.ToString(); 

            await context.CallActivityAsync("PullRequestStateUtility_CreateOrUpdatePullRequestState", pullRequestState);

            return outputs;
        }

        protected async Task<PullRequestStateContext> ExecuteAsync(IDurableOrchestrationContext context, PullRequestStateContext pullRequestDetailContext,
            PRCommentCreated comment, JObject parentReviewComment)
        {
            if (!pullRequestDetailContext.HasCreatedWorkItem(comment.comment.id))
            {
                WorkItem createdWorkItem =
                    await context.CallActivityAsync<WorkItem>(nameof(CreateWorkItemCommand) +"_CreateWorkItem",
                        parentReviewComment);
                pullRequestDetailContext.Add(
                    new CreatedWorkItem()
                    {
                        CommentId = comment.comment.id
                    });
            }

            return pullRequestDetailContext;
        }

        [FunctionName(nameof(CreateWorkItemCommand)+"_GetParentReview")]
        public async Task<JObject> GetParentReviewAsync([ActivityTrigger] PRCommentCreated comment, ILogger log)
        {

            try
            {
                // GitHub Client Library's domain object can't serializable.
                var result =  await _gitHubRepository.GetSingleComment(comment.comment.in_reply_to_id);
                return result.ToJObject();
            }
            catch (Exception e)
            {
                // GitHub Client Library's exception can't serializable
                throw new ArgumentException(e.Message);
            }
        }

        [FunctionName(nameof(CreateWorkItemCommand)+"_CreateWorkItem")]
        public async Task<WorkItem> CreateWorkItem(
            [ActivityTrigger] JObject parentReviewComment,
            ILogger log
        )
        {
            var workItem = new WorkItemSource()
            {
                Title = $"SonarCloud Issue [{parentReviewComment["PullRequestReviewId"]}][{parentReviewComment["Id"]}]",
                Description = parentReviewComment["Body"].ToString()
            };
            WorkItem createdWorkItem = await _workItemRepository.CreateWorkItem(workItem);
            return createdWorkItem;
        }

        [FunctionName(nameof(CreateWorkItemCommand) + "_GetPullRequestStateContext")]
        public async Task<EntityStateResponse<PullRequestStateContext>> GetPullRequestStateContext(
            [ActivityTrigger] string entityId,
            [OrchestrationClient] IDurableOrchestrationClient client,
            ILogger log
        )
        {
            return await client.ReadEntityStateAsync<PullRequestStateContext>(new EntityId(nameof(PullRequestEntity), entityId));
        }

    }
}