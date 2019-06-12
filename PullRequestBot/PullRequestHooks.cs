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
using PullRequestLibrary.Generated.GitHub.PRCommentCreated;

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

        [FunctionName("GitHubPRCommentHook")]
        public async Task<IActionResult> GitHubPRCommentHook(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);
            // Parse the request 
            var comment = JsonConvert.DeserializeObject<PRCommentCreated>(requestBody);
            // comment.comment.body
            // Parse the command 

            // Get the parent comment recursively
            // Get the Single comment of the parent
            // Create a work item  

            return (ActionResult)new OkObjectResult($"Done");
        }
    }
}
