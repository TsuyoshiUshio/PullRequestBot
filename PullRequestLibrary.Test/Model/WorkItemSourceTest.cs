
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.Text;
using PullRequestLibrary.Model;
using Xunit;

namespace PullRequestLibrary.Test.Model
{
    public class WorkItemSourceTest
    {
        [Fact]
        public void ConvertToJsonPatchDocument()
        {
            var workItem = new WorkItemSource()
            {
                Title = "foo",
                Description = "bar"
            };

            JsonPatchDocument document = workItem.ToJsonPatchDocument();


            // Title
            Assert.Equal(Operation.Add, document[0].Operation);
            Assert.Equal("/fields/System.Title", document[0].Path);
            Assert.Equal("foo", document[0].Value as string);
            // Description
            Assert.Equal(Operation.Add, document[1].Operation);
            Assert.Equal("/fields/Microsoft.VSTS.TCM.SystemInfo", document[1].Path);
            Assert.Equal("bar", document[1].Value as string);
        }
    }
}
