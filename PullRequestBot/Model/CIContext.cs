using System;
using System.Collections.Generic;
using System.Text;

namespace PullRequestBot.Model
{
    public class CIContext
    {
        public string PullRequestId { get; set; }
        public string ProjectKey { get; set; }
        public string CommitId { get; set; }
    }
}
