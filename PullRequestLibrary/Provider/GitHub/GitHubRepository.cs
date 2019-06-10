using Octokit;
using PullRequestLibrary.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PullRequestLibrary.Provider.GitHub
{
    public interface IGitHubRepository
    {
        Task<PullRequestReviewComment> CreatePullRequestReviewComment(Comment comment);
    }

    public class GitHubRepository : IGitHubRepository
    {
        internal IGitHubClient client; // internal for testability
        public GitHubRepository(IGitHubClient client)
        {
            this.client = client;
        }

        public Task<PullRequestReviewComment> CreatePullRequestReviewComment(Comment comment)
        {
            var reviewComment = new PullRequestReviewCommentCreate(comment.Body, comment.CommitId, comment.Path, comment.Position);
            return client.PullRequest.ReviewComment.Create(comment.RepositoryOnwer, comment.RepositoryName, int.Parse(comment.PullRequestId), reviewComment);
        }

    }
}
