using System;
using System.Collections.Generic;
using System.Text;

namespace PullRequestBot.Command.PullRequestStateUtility
{
    public class PullRequestStateKey
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
    }
}
