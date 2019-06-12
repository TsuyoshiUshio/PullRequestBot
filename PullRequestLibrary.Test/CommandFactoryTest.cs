using System;
using System.Collections.Generic;
using System.Text;
using PullRequestLibrary.Command;
using Xunit;

namespace PullRequestLibrary.Test
{

    public class CommandFactoryTest
    {
        [Fact]
        public void CreateWorkItemTest()
        {
            ICommand command = CommandFactory.Create(PRCommand.WorkItem);
            Assert.Equal<Type>(typeof(CreateWorkItemCommand), command.GetType());
        }
    }
}
