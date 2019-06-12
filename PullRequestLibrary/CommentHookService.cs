using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PullRequestLibrary.Command;
using PullRequestLibrary.Generated.GitHub.PRCommentCreated;

namespace PullRequestLibrary
{
    public class CommentHookService
    {
        public const string PRCommandCreateWorkItemCommand = "/workitem";

        private ICommandContext context;

        public CommentHookService(ICommandContext context)
        {
            this.context = context;
        }

        public async Task ExecuteCommentAsync(PRCommentCreated comment)
        {
            // comment.comment.body
            // Parse the command 

            // Get the parent comment recursively
            // Get the Single comment of the parent
            // Create a work item

            return;
        }

        internal PRCommand Parse(string pRCommand)
        {
            if (pRCommand?.Trim() == PRCommandCreateWorkItemCommand)
            {
                return PRCommand.WorkItem;
            }
            else
            {
                // there is no much. 
                return PRCommand.DoNothing;
            }
        }

        

        

    }
}
