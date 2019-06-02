using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace PullRequestBot
{
    public static class PullRequestHooks
    {
        [FunctionName("CreatePullRequest")]
        public static async Task<IActionResult> CreatePullRequestAsync(
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
        public static async Task<IActionResult> PollingAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [OrchestrationClient]IDurableOrchestrationClient client,
            ILogger log)
        {

            return (ActionResult)new OkObjectResult($"Polling Executed");
        }


    }
}
