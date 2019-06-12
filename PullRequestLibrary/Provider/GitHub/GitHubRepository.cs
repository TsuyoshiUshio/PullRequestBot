using Octokit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PullRequestLibrary.Model;

namespace PullRequestLibrary.Provider.GitHub
{
    public interface IGitHubRepository
    {
        Task<PullRequestReviewComment> CreatePullRequestReviewComment(Comment comment);
    }

    public class GitHubRepository : IGitHubRepository
    {
        internal IGitHubClient client; // internal for testability
        internal IGitHubRepositoryContext context;
        public GitHubRepository(IGitHubClient client, IGitHubRepositoryContext context)
        {
            this.client = client;
            this.context = context;
        }

        public Task<PullRequestReviewComment> CreatePullRequestReviewComment(Comment comment)
        {
            var reviewComment = new PullRequestReviewCommentCreate(comment.Body, comment.CommitId, comment.Path, comment.Position);
            return client.PullRequest.ReviewComment.Create(comment.RepositoryOnwer, comment.RepositoryName, int.Parse(comment.PullRequestId), reviewComment);
        }

        public Task<PullRequestReviewComment> GetSingleComment(int commentId)
        {
            return client.PullRequest.ReviewComment.GetComment(context.Owner, context.Name, commentId);
        }

    }
}
