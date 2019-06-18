using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;
using PullRequestBot.Command.PullRequestStateUtility;
using PullRequestBot.Entity;
using PullRequestBot.Model;
using PullRequestLibrary.Generated.SonarCloud.SearchIssue;
using PullRequestLibrary.Model;
using PullRequestLibrary.Provider.GitHub;
using PullRequestLibrary.Provider.SonarCloud;

namespace PullRequestBot.Decorator.CreatePRReviewDecorator
{
    public class CreatePRReviewDecorator
    {
        private IGitHubRepository _gitHubRepository;
        private IGitHubRepositoryContext _repositoryContext;
        private ISonarCloudRepository _sonarCloudRepository;

        public CreatePRReviewDecorator(IGitHubRepository gitHubRepository, IGitHubRepositoryContext repositoryContext,
            ISonarCloudRepository sonarCloudRepository)
        {
            this._gitHubRepository = gitHubRepository;
            this._repositoryContext = repositoryContext;
            this._sonarCloudRepository = sonarCloudRepository;
        }

        [FunctionName(nameof(CreatePRReviewDecorator))]
        public async Task Orchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {

            var cIContext = context.GetInput<CIContext>();
            // Get issues
            SearchIssue issues = await context.CallActivityAsync<SearchIssue>(nameof(CreatePRReviewDecorator) + "_GetIssues", cIContext);

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
                    nameof(CreatePRReviewDecorator) + "_GetPullRequestStateContext", entityId);

            var pullRequestDetailContext = response.EntityState;

            pullRequestDetailContext = pullRequestDetailContext ?? new PullRequestStateContext()
            {
                PullRequestNumber = int.Parse(cIContext.PullRequestId)
            };

            var unCommentedIssue = issues.issues.Where(issue => !pullRequestDetailContext.CreatedReviewComment.Contains(new CreatedReviewComment() {IssueId = issue.key})).ToList();

            
            foreach (var issue in unCommentedIssue)
            {
                var issueContext = (cIContext, issue);
                var createdPrReviewComment = await context.CallActivityAsync<JObject>(nameof(CreatePRReviewDecorator) + "_CreatePRReviewComment", issueContext);

                if (createdPrReviewComment != null)
                {
                    pullRequestDetailContext.Add(new CreatedReviewComment()
                        { IssueId = issue.key, CommentId = (int)createdPrReviewComment["Id"], ProjectKey = cIContext.ProjectKey});

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


        [FunctionName(nameof(CreatePRReviewDecorator) + "_CreatePRReviewComment")]
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
                        Body = $"**{issue.type}**\n> {issue.message}\n See [details](https://sonarcloud.io/project/issues?id={cIContext.ProjectKey}&open={issue.key}&pullRequest={cIContext.PullRequestId}&resolved=false)",
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

        [FunctionName(nameof(CreatePRReviewDecorator) + "_GetIssues")]
        public async Task<SearchIssue> CreateWorkItem(
            [ActivityTrigger] CIContext context,
            ILogger log
        )
        {
            return await _sonarCloudRepository.GetIssues(context.PullRequestId, context.ProjectKey);
            
        }

        [FunctionName(nameof(CreatePRReviewDecorator) + "_GetPullRequestStateContext")]
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
