using System;
using System.Collections.Generic;
using System.Text;
using PullRequestLibrary.Command;
using Xunit;

namespace PullRequestLibrary.Test
{
    public class CommandHookServiceTest
    {
        [Fact]
        public void ParseWorkItem()
        {
            var command = new CommentHookService();
            var result = command.Parse(CommentHookService.PRCommandCreateWorkItemCommand);
            Assert.Equal(PRCommand.WorkItem, result);
            result = command.Parse(CommentHookService.PRCommandCreateWorkItemCommand);
            Assert.Equal(PRCommand.WorkItem, result);
        }

        [Fact]
        public void ParseDoNothing()
        {
            var command = new CommentHookService();
            var result = command.Parse("/foo, /bar, /baz");
            Assert.Equal(PRCommand.DoNothing, result);
        }
    }
}
