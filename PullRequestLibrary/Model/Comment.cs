using System;
using System.Collections.Generic;
using System.Text;

namespace PullRequestLibrary.Model
{
    public class Comment
    {
        public string Body { get; set; }
        public string CommitId { get; set; }
        public string Path { get; set; }
        public int Position { get; set; }

        public string RepositoryOnwer { get; set; }
        public string RepositoryName { get; set; }
        public string PullRequestId { get; set; }

    }
}
