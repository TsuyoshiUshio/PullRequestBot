using System;
using System.Collections.Generic;
using System.Text;

namespace PullRequestLibrary.Model
{
    public class PullRequest
    {
        // Repository Id 
        public string PartitionKey { get; set; }
        // Pull Request Id 
        public string RowKey { get; set; }
        // Status (Open/Close)
        public string Status { get; set; }
        // Guid as a key of Entity Id. 
        public string EntityId { get; set; }
    }
}
