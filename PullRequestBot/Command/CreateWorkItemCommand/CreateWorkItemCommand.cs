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
            // PullRequestReviewComment can't deserialize. 
            var parentReviewComment = await context.CallActivityAsync<JObject>("CreateWorkItemCommand_GetParentReview", comment);
            // Get the State of the PullRequestState
            var pullRequestState =
                await context.CallActivityAsync<PullRequestState>("PullRequestStateUtility_GetPullRequestState", new PullRequestStateKey {
                    PartitionKey = comment.repository.full_name.ToPartitionKey(),
                    RowKey = comment.pull_request.id.ToString()});
            
            // Ask the entity has duplication  
            string entityId = pullRequestState?.EntityId ?? context.NewGuid().ToString();
            var pullRequestDetailContext = await context.CallEntityAsync<PullRequestStateContext>(new EntityId(nameof(PullRequestEntity), entityId), "get", new PullRequestStateContext());

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
                new EntityId(nameof(PullRequestEntity), entityId), "update", pullRequestDetailContext);

            // update Status.

            pullRequestState = pullRequestState ?? new PullRequestState();
            pullRequestState.EntityId = entityId;
            pullRequestState.PartitionKey = pullRequestState.PartitionKey ?? comment.repository.full_name.ToPartitionKey();
            pullRequestState.RowKey = pullRequestState.RowKey ?? comment.pull_request.number.ToString(); 

            await context.CallActivityAsync("PullRequestStateUtility_CreateOrUpdatePullRequestState", pullRequestState);

            return outputs;
        }

        [FunctionName("CreateWorkItemCommand_GetParentReview")]
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

        [FunctionName("CreateWorkItemCommand_CreateWorkItem")]
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

    }
}