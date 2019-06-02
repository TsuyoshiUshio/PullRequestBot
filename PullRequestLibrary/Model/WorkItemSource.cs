using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace PullRequestLibrary.Model
{
    public class WorkItemSource
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public JsonPatchDocument ToJsonPatchDocument()
        {
            var document = new JsonPatchDocument();

            document.Add(new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.Title",
                Value = Title
            });

            document.Add(new JsonPatchOperation()
            {
                Operation = Operation.Add,
                Path = "/fields/System.Description",
                Value = Description
            });
            return document;
        }
    }
}
