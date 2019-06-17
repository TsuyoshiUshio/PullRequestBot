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
    public class CreateWorkItemCommand : CommandBaseCommand.CommandBaseCommand
    {
        private readonly IWorkItemRepository _workItemRepository;

        public CreateWorkItemCommand(IGitHubRepository gitHubRepository, IWorkItemRepository workItemRepository) : base(gitHubRepository)
        {
            _workItemRepository = workItemRepository;
        }

        [FunctionName(nameof(CreateWorkItemCommand))]
        public override Task<List<string>> EntryPoint(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            return RunOrchestrator(context);
        }

        protected override async Task<PullRequestStateContext> ExecuteAsync(IDurableOrchestrationContext context, PullRequestStateContext pullRequestDetailContext,
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

    }
}