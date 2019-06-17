using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using PullRequestLibrary.Provider.GitHub;
using PullRequestLibrary.Provider.SonarCloud;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;
using PullRequestBot.Command.PullRequestStateUtility;
using PullRequestBot.Entity;
using PullRequestBot.Model;
using PullRequestLibrary.Generated.SonarCloud.SearchIssue;
using PullRequestLibrary.Model;

namespace PullRequestBot.Command.CreatePRReviewCommand
{
    public class CreatePRReviewCommand
    {
        private IGitHubRepository _gitHubRepository;
        private IGitHubRepositoryContext _repositoryContext;
        private ISonarCloudRepository _sonarCloudRepository;

        public CreatePRReviewCommand(IGitHubRepository gitHubRepository, IGitHubRepositoryContext repositoryContext,
            ISonarCloudRepository sonarCloudRepository)
        {
            this._gitHubRepository = gitHubRepository;
            this._repositoryContext = repositoryContext;
            this._sonarCloudRepository = sonarCloudRepository;
        }

        [FunctionName(nameof(CreatePRReviewCommand))]
        public async Task Orchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {

            var cIContext = context.GetInput<CIContext>();
            // Get issues
            SearchIssue issues = await context.CallActivityAsync<SearchIssue>(nameof(CreatePRReviewCommand) + "_GetIssues", cIContext);

            // Get issues that already created 
            var pullRequestState =
                await context.CallActivityAsync<PullRequestState>("PullRequestStateUtility_GetPullRequestState", new PullRequestStateKey
                {
                    PartitionKey = _repositoryContext.GetFullNameWithUnderscore(),
                    RowKey = cIContext.PullRequestId
                });

            string entityId = pullRequestState?.EntityId ?? context.NewGuid().ToString();
            EntityStateResponse<PullRequestStateContext> response =
                await context.CallActivityAsync<EntityStateResponse<PullRequestStateContext>>(
                    nameof(CreatePRReviewCommand) + "_GetPullRequestStateContext", entityId);

            var pullRequestDetailContext = response.EntityState;

            pullRequestDetailContext = pullRequestDetailContext ?? new PullRequestStateContext();

            var unCommentedIssue = issues.issues.Where(issue => !pullRequestDetailContext.CreatedReviewComment.Contains(new CreatedReviewComment() {IssueId = issue.key})).ToList();

            
            foreach (var issue in unCommentedIssue)
            {
                var issueContext = (cIContext, issue);
                var createdPrReviewComment = await context.CallActivityAsync<JObject>(nameof(CreatePRReviewCommand) + "_CreatePRReviewComment", issueContext);

                if (createdPrReviewComment != null)
                {
                    pullRequestDetailContext.Add(new CreatedReviewComment()
                        { IssueId = issue.key, CommentId = (int)createdPrReviewComment["Id"] });

                }
            }


            await context.CallEntityAsync<PullRequestStateContext>(new EntityId(nameof(PullRequestEntity), entityId), "update", pullRequestDetailContext);

            pullRequestState = pullRequestState ?? new PullRequestState();
            pullRequestState.EntityId = entityId;
            pullRequestState.PartitionKey = pullRequestState.PartitionKey ??
                                            _repositoryContext.Owner + "_" + _repositoryContext.Name;
            pullRequestState.RowKey = pullRequestState.RowKey ?? cIContext.PullRequestId;

            await context.CallActivityAsync("PullRequestStateUtility_CreateOrUpdatePullRequestState", pullRequestState);
        }


        [FunctionName(nameof(CreatePRReviewCommand) + "_CreatePRReviewComment")]
        public async Task<JObject> CreatePRReviewCommentAsync(
            [ActivityTrigger] IDurableActivityContext context)
        {
            var issueContext = context.GetInput<Tuple<CIContext, PullRequestLibrary.Generated.SonarCloud.SearchIssue.Issue>>();
            var cIContext = issueContext.Item1;
            var issue = issueContext.Item2;


            var path = issue.component.Replace($"{cIContext.ProjectKey}:", "");
            if (path != cIContext.ProjectKey)
            {
                PullRequestReviewComment result = await _gitHubRepository.CreatePullRequestReviewComment(
                    new PullRequestLibrary.Model.Comment
                    {
                        Body = $"[{issue.type}] {issue.message}",
                        RepositoryOnwer = _repositoryContext.Owner,
                        RepositoryName = _repositoryContext.Name,
                        CommitId = cIContext.CommitId,
                        Path = path,
                        Position = 5,
                        PullRequestId = cIContext.PullRequestId
                    });
                
                var json = JsonConvert.SerializeObject(result);
                return JObject.Parse(json);
            }

            return null;
        }

        [FunctionName(nameof(CreatePRReviewCommand) + "_GetIssues")]
        public async Task<SearchIssue> CreateWorkItem(
            [ActivityTrigger] CIContext context,
            ILogger log
        )
        {
            return await _sonarCloudRepository.GetIssues(context.PullRequestId, context.ProjectKey);
            
        }

        [FunctionName(nameof(CreatePRReviewCommand) + "_GetPullRequestStateContext")]
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
