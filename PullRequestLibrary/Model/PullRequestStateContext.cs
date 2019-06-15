using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PullRequestLibrary.Model
{
    public class PullRequestStateContext
    { 
        public PullRequestStateContext()
        {
            CreatedWorkItem = new List<CreatedWorkItem>();
            CreatedReviewComment = new List<CreatedReviewComment>();
        }

        public List<CreatedWorkItem> CreatedWorkItem { get; set; }
        public List<CreatedReviewComment> CreatedReviewComment { get; set; }

        public Boolean HasCreatedWorkItem(int commentId)
        {
            return (CreatedWorkItem.Count(x => x.CommentId == commentId) == 1);
        }

        public void Add(CreatedWorkItem workItem)
        {
            CreatedWorkItem.Add(workItem);
        }

        public void Add(CreatedReviewComment comment)
        {
            CreatedReviewComment.Add(comment);
        }
    }
}
