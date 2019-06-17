using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PullRequestLibrary.Generated.GitHub.PRCommentCreated;
using PullRequestLibrary.Provider.GitHub;
using Newtonsoft.Json.Linq;
using PullRequestBot.Command.PullRequestStateUtility;
using PullRequestBot.Entity;
using PullRequestLibrary.Model;
using PullRequest = Octokit.PullRequest;

namespace PullRequestBot.Command.CommandBaseCommand
{
    public abstract class CommandBaseCommand
    {
        protected readonly IGitHubRepository _gitHubRepository;

        protected CommandBaseCommand(IGitHubRepository gitHubRepository)
        {
            this._gitHubRepository = gitHubRepository;
        }

        public abstract  Task<List<string>> EntryPoint(
            [OrchestrationTrigger] IDurableOrchestrationContext context);

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
            var parentReviewComment = await context.CallActivityAsync<JObject>(nameof(CommandBaseCommand) +"_GetParentReview", comment);
            // Get the State of the PullRequestState
            var pullRequestState =
                await context.CallActivityAsync<PullRequestState>("PullRequestStateUtility_GetPullRequestState", new PullRequestStateKey {
                    PartitionKey = comment.repository.full_name.ToPartitionKey(),
                    RowKey = comment.pull_request.id.ToString()});
            
            // Ask the entity has duplication  
            string entityId = pullRequestState?.EntityId ?? context.NewGuid().ToString();
            EntityStateResponse<PullRequestStateContext> response =
                await context.CallActivityAsync<EntityStateResponse<PullRequestStateContext>>(
                    nameof(CommandBaseCommand) + "_GetPullRequestStateContext", entityId);

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

        protected abstract Task<PullRequestStateContext> ExecuteAsync(IDurableOrchestrationContext context,
            PullRequestStateContext pullRequestDetailContext,
            PRCommentCreated comment, JObject parentReviewComment);
    }
}