using PullRequestLibrary.Generated.SonarCloud.SearchIssue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PullRequestLibrary.Provider.SonarCloud
{
    public interface ISonarCloudRepository
    {
        Task<SearchIssue> GetIssues(string pullRequestId, string projectKey);
    }

    public class SonarCloudRepository : ISonarCloudRepository
    {
        private IRestClientContext context;
        public SonarCloudRepository(IRestClientContext context)
        {
            this.context = context;
        }

        public Task<SearchIssue> GetIssues(string pullRequestId, string projectKey)
        {
            return context.GetAsync<SearchIssue>($"https://sonarcloud.io/api/issues/search?pullRequest={pullRequestId}&projects={projectKey}");
        }
    }
}
