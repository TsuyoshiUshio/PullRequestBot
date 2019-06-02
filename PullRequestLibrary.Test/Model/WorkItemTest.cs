using PullRequestLibrary.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PullRequestLibrary.Test.Model
{
    public class WorkItemTest
    {
        [Fact]
        public void ConvertToWorkItemFields()
        {
            var workItem = new WorkItem()
            {
                Title = "foo",
                Description = "bar"
            };
            List<WorkItemField> workItemFields = workItem.ToWorkItemFields();
            // Title
            Assert.Equal("add", workItemFields[0].Operation);
            Assert.Equal("/fields/System.Title", workItemFields[0].Path);
            Assert.Null(workItemFields[0].From);
            Assert.Equal("foo", workItemFields[0].Value as string);
            // Description
            Assert.Equal("add", workItemFields[1].Operation);
            Assert.Equal("/fields/System.Description", workItemFields[1].Path);
            Assert.Null(workItemFields[1].From);
            Assert.Equal("bar", workItemFields[1].Value as string);
        }
    }
}
