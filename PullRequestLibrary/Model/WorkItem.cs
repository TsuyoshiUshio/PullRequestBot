using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PullRequestLibrary.Model
{
    public class WorkItemField
    {
        [JsonProperty("op")]
        public string Operation { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("from")]
        public string From { get; set; }
        [JsonProperty("value")]
        public object Value { get; set; }
    }

    public class WorkItem
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public List<WorkItemField> ToWorkItemFields()
        {
            var result = new List<WorkItemField>();
            // Title 
            result.Add(
                new WorkItemField()
                {
                    Operation = "add",
                    Path = "/fields/System.Title",
                    Value = Title
                });
            // Description
            result.Add(
                new WorkItemField()
                {
                    Operation = "add",
                    Path = "/fields/System.Description",
                    Value = Description
                }
                );
            return result;
        }
    }
}
