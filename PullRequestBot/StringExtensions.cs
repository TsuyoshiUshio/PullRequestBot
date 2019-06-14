using System;
using System.Collections.Generic;
using System.Text;

namespace PullRequestBot
{
    public static class StringExtensions
    {
        public static string ToPartitionKey(this string repositoryFullName)
        {
            return repositoryFullName.Replace("/", "_");
        }
    }
}
