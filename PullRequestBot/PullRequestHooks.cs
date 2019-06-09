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
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Dynamitey;

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
        ILogger log)
        {
            string pullRequestId = req.Query["pullRequestId"];
            string projectKey = req.Query["projectKey"];
            string commitId = req.Query["commitId"];
            log.LogInformation($"PullRequestId: {pullRequestId} ProjectKey: {projectKey}");
            await service.GenerateAnalysisComment(pullRequestId, projectKey, commitId);
            return (ActionResult)new OkObjectResult($"Done");
        }

        [FunctionName("CreatePullRequest")]
        public async Task<IActionResult> CreatePullRequestAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [OrchestrationClient]IDurableOrchestrationClient client,
            ILogger log)
        {

            // Get the original webhook request
            // Research the structure of the PullRequest, Comment structure by sending request. 

            // Receive the Pull Request Id 
            // Start an orchestration to create an durable entitiy with id. 


            log.LogInformation("C# HTTP trigger function processed a request.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            return (ActionResult)new OkObjectResult($"Pull Request Registered.");
        }

        // This method will be replaced with Timer Trigger. 
        // During the time of debugging, I put it on the 
        [FunctionName("Polling")]
        public async Task<IActionResult> PollingAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [OrchestrationClient]IDurableOrchestrationClient client,
            ILogger log)
        {

            return (ActionResult)new OkObjectResult($"Polling Executed");
        }


    }
}
