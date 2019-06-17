using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PullRequestLibrary;
using PullRequestLibrary.Generated.GitHub.PRCommentCreated;
using PullRequestBot.Command;
using PullRequestBot.Command.CreatePRReviewCommand;
using PullRequestBot.Command.CreateWorkItemCommand;
using PullRequestBot.Model;

namespace PullRequestBot
{
    public class PullRequestHooks
    {
        private ICIHookService service;

        public PullRequestHooks(ICIHookService service)
        {
            this.service = service;
        }

        [FunctionName("CIHook")]
        public async Task<IActionResult> CIHook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        [OrchestrationClient]IDurableOrchestrationClient starter,
        ILogger log)
        {
            string pullRequestId = req.Query["pullRequestId"];
            string projectKey = req.Query["projectKey"];
            string commitId = req.Query["commitId"];
            log.LogInformation($"PullRequestId: {pullRequestId} ProjectKey: {projectKey}");
            var cIContext = new CIContext()
            {
                PullRequestId = pullRequestId,
                ProjectKey = projectKey,
                CommitId = commitId
            };
            var instanceId = await starter.StartNewAsync(nameof(CreatePRReviewCommand), cIContext);
            DurableOrchestrationStatus status = await starter.GetStatusAsync(instanceId, false, false);
            return (ActionResult)new OkObjectResult(status);
        }

        [FunctionName("GitHubPRCommentHook")]
        public async Task<IActionResult> GitHubPRCommentHook(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            [OrchestrationClient]IDurableOrchestrationClient starter,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);
            // Parse the request 
            var comment = JsonConvert.DeserializeObject<PRCommentCreated>(requestBody);

            // Start Orchestrator
            var commandName = comment.CommandName();
            
            if (!string.IsNullOrEmpty(commandName))
            {
                string instanceId = await starter.StartNewAsync(nameof(CreateWorkItemCommand), comment);
                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
                DurableOrchestrationStatus status = await starter.GetStatusAsync(instanceId, false, false);
                return (ActionResult) new OkObjectResult(status);
            } else
            {
                var status = new DurableOrchestrationStatus();
                status.RuntimeStatus = OrchestrationRuntimeStatus.Completed;
                return (ActionResult) new OkObjectResult(status);
            }

        }

    }
}
