using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImpromptuInterface.Optimization;
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
using PullRequestLibrary.Provider.SonarCloud;
using PullRequest = Octokit.PullRequest;

namespace PullRequestBot.Command.CreateWorkItemCommand
{
    public class CreateWorkItemCommand : CommandBaseCommand.CommandBaseCommand
    {
        private readonly IWorkItemRepository _workItemRepository;
        private readonly ISonarCloudRepository _sonarCloudRepository;

        public CreateWorkItemCommand(IGitHubRepository gitHubRepository, IWorkItemRepository workItemRepository, ISonarCloudRepository sonarCloudRepository) : base(gitHubRepository)
        {
            _workItemRepository = workItemRepository;
            _sonarCloudRepository = sonarCloudRepository;
        }

        [FunctionName(nameof(CreateWorkItemCommand))]
        public override Task<List<string>> EntryPoint(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            return RunOrchestrator(context);
        }

        protected override async Task<PullRequestStateContext> ExecuteAsync(IDurableOrchestrationContext context,
            PullRequestStateContext pullRequestDetailContext,
            PRCommentCreated comment, JObject parentReviewComment, EntityId entityId)
        {
            if (!pullRequestDetailContext.HasCreatedWorkItem(comment.comment.id))
            {
                WorkItem createdWorkItem =
                    await context.CallActivityAsync<WorkItem>(nameof(CreateWorkItemCommand) +"_CreateWorkItem",
                        (parentReviewComment, pullRequestDetailContext));
                pullRequestDetailContext.Add(
                    new CreatedWorkItem()
                    {
                        CommentId = comment.comment.id
                    });

                var createReplyparameter = new CreateReplyParamter()
                {
                    PullRequestNumber = comment.pull_request.number,
                    InReplyTo = (int)parentReviewComment["Id"],
                    WorkItem = createdWorkItem
                };

                await context.CallActivityAsync(nameof(CreateWorkItemCommand) + "_CreateReplyComment",
                    createReplyparameter);
            }

            return pullRequestDetailContext;
        }


        [FunctionName(nameof(CreateWorkItemCommand)+"_CreateWorkItem")]
        public async Task<WorkItem> CreateWorkItem(
            [ActivityTrigger] Tuple<JObject, PullRequestStateContext> createWorkItemParameter,
            ILogger log
        )
        {

            var parentReviewComment = createWorkItemParameter.Item1;
            var pullRequestStateContext = createWorkItemParameter.Item2;


            var currentReviewComment = pullRequestStateContext.CreatedReviewComment
                .FirstOrDefault(p => p.CommentId == (int) parentReviewComment["Id"]);
            var issues =
                await _sonarCloudRepository.GetIssues(pullRequestStateContext.PullRequestNumber.ToString(), currentReviewComment.ProjectKey, currentReviewComment.IssueId);
            var currentIssue = issues.issues.FirstOrDefault();

            var body = $"<b>{currentIssue.type}</b></p>{currentIssue.message}</p>See more <a href=\"https://sonarcloud.io/project/issues?id={currentReviewComment.ProjectKey}&open={currentIssue.key}&pullRequest={pullRequestStateContext.PullRequestNumber}&resolved=false\">details</a>.";
            var workItem = new WorkItemSource()
            {
                Title = $"SonarCloud GitHub Issue PR {pullRequestStateContext.PullRequestNumber}: {currentIssue.message}",
                Description = body
            };
            WorkItem createdWorkItem = await _workItemRepository.CreateWorkItem(workItem);

            return createdWorkItem;
        }

        [FunctionName(nameof(CreateWorkItemCommand) + "_CreateReplyComment")]
        public Task CreateReplyComment([ActivityTrigger] CreateReplyParamter parameter)
        {
            var body = $"WorkItem {parameter.WorkItem.Id} Created see [workItem]({((Microsoft.VisualStudio.Services.WebApi.ReferenceLink)parameter.WorkItem.Links.Links["html"]).Href}).";

            return _gitHubRepository.CreatePullRequestReplyComment(parameter.PullRequestNumber, body,
                parameter.InReplyTo);
        }

        public class CreateReplyParamter
        {
            public int PullRequestNumber { get; set; }
            public int InReplyTo { get; set; }

            public WorkItem WorkItem { get; set; }
        }

    }
}