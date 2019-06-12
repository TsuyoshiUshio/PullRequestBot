using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using PullRequestLibrary.Command;
using Xunit;

namespace PullRequestLibrary.Test
{
    public class CommandHookServiceTest
    {
        [Fact]
        public void ParseWorkItem()
        {
            var contextMock = new Mock<ICommandContext>();
            var command = new CommentHookService(contextMock.Object);
            var result = command.Parse(CommentHookService.PRCommandCreateWorkItemCommand);
            Assert.Equal(PRCommand.WorkItem, result);
            result = command.Parse(CommentHookService.PRCommandCreateWorkItemCommand);
            Assert.Equal(PRCommand.WorkItem, result);
        }

        [Fact]
        public void ParseDoNothing()
        {
            var contextMock = new Mock<ICommandContext>();
            var command = new CommentHookService(contextMock.Object);
            var result = command.Parse("/foo, /bar, /baz");
            Assert.Equal(PRCommand.DoNothing, result);
        }
    }
}
