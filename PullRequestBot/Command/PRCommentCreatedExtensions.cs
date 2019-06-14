using System;
using System.Collections.Generic;
using System.Text;
using PullRequestLibrary.Generated.GitHub.PRCommentCreated;

namespace PullRequestBot.Command
{
    public static class PRCommentCreatedExtensions
    {
        private const string CreateWorkItemComment = "/workitem";
        private const string CreateWorkItemCommand = "CreateWorkItemCommand";

        public static string CommandName(this PRCommentCreated comment)
        {
            var rawComment = comment?.comment?.body;

            if (rawComment?.Trim() == CreateWorkItemComment)
            {
                return CreateWorkItemCommand;
            }
            else
            {
                // there is no much. 
                return null;
            }
        }
    }
}
