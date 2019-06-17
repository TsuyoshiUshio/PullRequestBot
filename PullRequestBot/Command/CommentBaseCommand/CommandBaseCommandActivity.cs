using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PullRequestLibrary.Generated.GitHub.PRCommentCreated;
using PullRequestLibrary.Provider.GitHub;
using Newtonsoft.Json.Linq;
using PullRequestBot.Command.PullRequestStateUtility;
using PullRequestBot.Entity;
using PullRequestLibrary.Model;
using PullRequest = Octokit.PullRequest;

namespace PullRequestBot.Command.CommandBaseCommand
{
    public class CommandBaseCommandActivity
    {
        protected readonly IGitHubRepository _gitHubRepository;

        public CommandBaseCommandActivity(IGitHubRepository gitHubRepository)
        {
            this._gitHubRepository = gitHubRepository;
        }

        [FunctionName(nameof(CommandBaseCommand) +"_GetParentReview")]
        public async Task<JObject> GetParentReviewAsync([ActivityTrigger] PRCommentCreated comment, ILogger log)
        {

            try
            {
                // GitHub Client Library's domain object can't serializable.
                var result =  await _gitHubRepository.GetSingleComment(comment.comment.in_reply_to_id);
                return result.ToJObject();
            }
            catch (Exception e)
            {
                // GitHub Client Library's exception can't serializable
                throw new ArgumentException(e.Message);
            }
        }

        [FunctionName(nameof(CommandBaseCommand) + "_GetPullRequestStateContext")]
        public async Task<EntityStateResponse<PullRequestStateContext>> GetPullRequestStateContext(
            [ActivityTrigger] string entityId,
            [OrchestrationClient] IDurableOrchestrationClient client,
            ILogger log
        )
        {
            return await client.ReadEntityStateAsync<PullRequestStateContext>(new EntityId(nameof(PullRequestEntity), entityId));
        }

    }
}