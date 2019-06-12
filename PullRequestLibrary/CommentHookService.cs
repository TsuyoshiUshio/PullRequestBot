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
            PRCommand pRCommand = Parse(comment?.comment?.body);
            var command = CommandFactory.Create(pRCommand);
            context.PRCommentCreated = comment;
            await command.Execute(context);

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
