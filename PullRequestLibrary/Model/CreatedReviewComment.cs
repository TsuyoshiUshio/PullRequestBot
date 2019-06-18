using System;
using System.Collections.Generic;
using System.Text;

namespace PullRequestLibrary.Model
{
    public class CreatedReviewComment
    {
        public int CommentId { get; set; }
        public string IssueId { get; set; }
        public string ProjectKey { get; set; }
    }
}
