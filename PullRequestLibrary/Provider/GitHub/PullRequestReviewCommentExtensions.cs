using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;

namespace PullRequestLibrary.Provider.GitHub
{
    public static class PullRequestReviewCommentExtensions
    { 
        public static JObject ToJObject(this PullRequestReviewComment comment)
        {
            var json = JsonConvert.SerializeObject(comment);
            return JObject.Parse(json);
        }
    }
}
