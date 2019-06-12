using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PullRequestLibrary.Command
{
    public class DoNothingCommand : ICommand
    {
        public Task Execute(ICommandContext context)
        {
            // this command do nothing.
            return Task.FromResult<string>("");
        }
    }
}
