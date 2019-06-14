using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace PullRequestLibrary.Model
{
    public class PullRequestState : TableEntity
    {
        // PartitionKey: Repository Full Name (e.g. TsuyoshiUshio/VulnerableApp)  
        // RowKey: Pull Request Id 

        // Status (Open/Close)
        public string Status { get; set; }
        // Guid as a key of Entity Id. 
        public string EntityId { get; set; }
    }
}
