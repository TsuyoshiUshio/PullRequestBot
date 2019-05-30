using Microsoft.Azure.WebJobs;
using PullRequestBot.Model;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace PullRequestBot
{
    public static class PullRequestOrchestrator
    {
        [FunctionName("RegisterPullRequestOrchestrator")]
        public static async Task<string> RegisterPullRequestOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context
            )
        {
            // Create an Actor
            // Create an Stroage Table

            // What do you store in here?
            // 1. the number of the PR and metadata (Azure DevOps / Organization / Project / Repo etc. Need more investiation)
            // 2. Already tracked event (e.g. create a work item by annotation of the comment. In this case, comment id will be the one.)

        }

        [FunctionName("PullRequestEntity")]
        public static async Task PullRequestEntity(
            [EntityTrigger] IDurableEntityContext context)
        {
            PullRequest currentState = context.GetState<PullRequest>();
            PullRequest input = context.GetInput<PullRequest>();

            switch(context.OperationName)
            {
                case "create":
                    currentState = input;
                    break;
            }
            context.SetState(currentState);
        }

        [FunctionName("CreatePullRequestTable")]
        public static async Task CreatePullRequestTableAsync(
            [ActivityTrigger] IDurableActivityContext context,
            [Table("PullRequest", "StorageConnectionString")] IAsyncCollector<PullRequestTable> pullRequests)
        {
            PullRequest request = context.GetInput<PullRequest>();
            await pullRequests.AddAsync(new PullRequestTable
            {
                PartitionKey = "key1",
                RowKey = "key2",
                State = "Created"
            });
        }
    }

    public class PullRequestTable
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string State { get; set; }
    }
}
