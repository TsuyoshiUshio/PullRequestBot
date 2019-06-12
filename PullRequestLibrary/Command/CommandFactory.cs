using System;
using System.Collections.Generic;
using System.Text;

namespace PullRequestLibrary.Command
{
    public enum PRCommand
    {
        WorkItem,
        DoNothing
    }

    public class CommandFactory
    {
        public static ICommand Create(PRCommand command)
        {
            
            return new CreateWorkItemCommand();
        }
    }
}
