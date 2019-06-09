using Octokit;
using PullRequestLibrary.Generated.SonarCloud.SearchIssue;
using PullRequestLibrary.Provider.GitHub;
using PullRequestLibrary.Provider.SonarCloud;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

        public CIHookService(IGitHubRepository gitHubRepository, ISonarCloudRepository sonarCloudRepository)
        {
            this.gitHubRepository = gitHubRepository;
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
                    await gitHubRepository.CreatePullRequestReviewComment(
                        new Model.Comment
                        {
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
