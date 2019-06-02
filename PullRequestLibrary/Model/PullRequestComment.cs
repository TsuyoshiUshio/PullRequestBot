using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PullRequestLibrary.Model
{
    public class PullRequestComment
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("parentCommentId")]
        public int ParentCommentId { get; set; }
        [JsonProperty("commentType")]
        public int CommentType { get; set; }
    }
}
