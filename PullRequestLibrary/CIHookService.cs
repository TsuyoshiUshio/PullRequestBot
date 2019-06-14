using Octokit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PullRequestLibrary.Generated.SonarCloud.SearchIssue;
using PullRequestLibrary.Provider.GitHub;
using PullRequestLibrary.Provider.SonarCloud;

namespace PullRequestLibrary
{
    public interface ICIHookService
    {
        Task GenerateAnalysisComment(string pullRequestId, string projectKey, string commitId);
    }

    public class CIHookService : ICIHookService
    {
        private IGitHubRepository gitHubRepository;
        private IGitHubRepositoryContext repositoryContext;
        private ISonarCloudRepository sonarCloudRepository;

        public CIHookService(IGitHubRepository gitHubRepository, IGitHubRepositoryContext repositoryContext, ISonarCloudRepository sonarCloudRepository)
        {
            this.gitHubRepository = gitHubRepository;
            this.repositoryContext = repositoryContext;
            this.sonarCloudRepository = sonarCloudRepository;
        }

        public async Task GenerateAnalysisComment(string pullRequestId, string projectKey, string commitId)
        {
            // Issues of SonarCloud
            SearchIssue searchIssue = await sonarCloudRepository.GetIssues(pullRequestId, projectKey);
            foreach (var issue in searchIssue.issues)
            {
                var path = issue.component.Replace($"{projectKey}:", "");
                if (path != projectKey)
                {
                    // TODO there is possibility of throttling. If it happens consider make it Activity and retry it. 
                    await gitHubRepository.CreatePullRequestReviewComment(
                        new Model.Comment
                        {
                            Body = $"[{issue.type}] {issue.message}",
                            RepositoryOnwer = repositoryContext.Owner,
                            RepositoryName = repositoryContext.Name,
                            CommitId = commitId,
                            Path = path,
                            Position = 5,
                            PullRequestId = pullRequestId
                        });
                }
            }
        }
    }
}
