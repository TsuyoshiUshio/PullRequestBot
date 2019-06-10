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
    }
}
