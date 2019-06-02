using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PullRequestLibrary
{
    public interface IPullRequestRepository
    {
        Task<Comment> CreatePRCommentAsync(Comment comment, string repositoryId, int pullRequestId, int threadId);
        Task<List<GitPullRequestCommentThread>> GetPRThreadAsync(string repositoryId, int pullRequestId);
    }

    public class PullRequestRepository : IPullRequestRepository
    {
        private GitHttpClientBase client;

        public PullRequestRepository(GitHttpClientBase client)
        {
            this.client = client;
        }

        public Task<List<GitPullRequestCommentThread>> GetPRThreadAsync(string repositoryId, int pullRequestId)
        {
            return client.GetThreadsAsync(repositoryId, pullRequestId);
        }

        public Task<Comment> CreatePRCommentAsync(Comment comment, string repositoryId, int pullRequestId, int threadId)
        {
            return client.CreateCommentAsync(comment, repositoryId, pullRequestId, threadId);
        }
    }
}
