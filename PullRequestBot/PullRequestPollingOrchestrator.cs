using Microsoft.Azure.WebJobs;
using PullRequestLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PullRequestBot
{
    public class PullRequestPollingOrchestrator
    {
        private IPullRequestRepository pullRequestRepository;
        private IWorkItemRepository workItemRepository;

        public PullRequestPollingOrchestrator(
            IPullRequestRepository pullRequestRepository,
            IWorkItemRepository workItemRepository)
        {
            this.pullRequestRepository = pullRequestRepository;
            this.workItemRepository = workItemRepository;
        }


        [FunctionName("ScanCommentOrchestrator")]
        public static async Task<string> ScanCommentOrchestrator(
    [OrchestrationTrigger] IDurableOrchestrationContext context
    )
        {

            return "";
        }
    }
}
