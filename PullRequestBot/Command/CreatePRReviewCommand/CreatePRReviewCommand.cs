using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using PullRequestLibrary.Provider.GitHub;
using PullRequestLibrary.Provider.SonarCloud;
using Microsoft.Extensions.Logging;
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
            var pullRequestDetailContext = await context.CallEntityAsync<PullRequestStateContext>(new EntityId(nameof(PullRequestEntity), entityId), "get", new PullRequestStateContext());

            pullRequestDetailContext = pullRequestDetailContext ?? new PullRequestStateContext();



            // Start SubOrchestrator with Not Created Issues

        }

        [FunctionName(nameof(CreatePRReviewCommand) + "_CreateReviewCommentSubOrchestrator")]
        public async Task CreateReviewCommentSubOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            // Create an issue
            // Store it to the Entity
            // 
             
        }

        [FunctionName(nameof(CreatePRReviewCommand) + "_GetIssues")]
        public async Task<SearchIssue> CreateWorkItem(
            [ActivityTrigger] CIContext context,
            ILogger log
        )
        {
            return await _sonarCloudRepository.GetIssues(context.PullRequestId, context.ProjectKey);
            
        }

    }
}
