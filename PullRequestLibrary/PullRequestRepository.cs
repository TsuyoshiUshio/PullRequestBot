using PullRequestLibrary.Generated.PRCreated;
using PullRequestLibrary.Generated.PRThread;
using PullRequestLibrary.Model;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace PullRequestLibrary
{
    public interface IPullRequestRepository
    {
        Task<PRCreated> CreatePRCommentAsync(string repositoryId, string pullRequestId, string threadId, PullRequestComment payload);
        Task<PRThread> GetPRThreadAsync(string repositoryId, string pullRequestId);
    }

    public class PullRequestRepository : IPullRequestRepository
    {
        private IRestClientContext context;
        public PullRequestRepository(IRestClientContext context)
        {
            this.context = context;
        }

        public Task<PRThread> GetPRThreadAsync(string repositoryId, string pullRequestId)
        {
            return context.GetAsync<PRThread>($"/csedevops/DevSecOps/_apis/git/repositories/{repositoryId}/pullRequests/{pullRequestId}/threads?api-version=5.0");
        }

        public Task<PRCreated> CreatePRCommentAsync(string repositoryId, string pullRequestId, string threadId, PullRequestComment payload)
        {
            return context.PostAsync<PullRequestComment, PRCreated>(
                $"/csedevops/DevSecOps/_apis/git/repositories/{repositoryId}/pullRequests/{pullRequestId}/threads/{threadId}/comments?api-version=5.0",
                payload);
        }
    }
}
