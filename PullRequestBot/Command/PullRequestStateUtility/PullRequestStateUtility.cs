using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using PullRequestLibrary.Generated.GitHub.PRCommentCreated;
using PullRequestLibrary.Model;

namespace PullRequestBot.Command.PullRequestStateUtility
{
    public class PullRequestStateUtility
    {
        [FunctionName("PullRequestStateUtility_GetPullRequestState")]
        public async Task<PullRequestState> GetPullRequestState(
            [ActivityTrigger] PullRequestStateKey key,
            [Table("PullRequestState")] CloudTable cloudTable,
            ILogger log)
        {
            TableQuery<PullRequestState> query = new TableQuery<PullRequestState>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, key.PartitionKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, key.RowKey)));
            var pullRequetStates = await cloudTable.ExecuteQuerySegmentedAsync(query, null);
            return pullRequetStates.Results.FirstOrDefault(); // Since we specify one record, it should be only one or null.
        }

        [FunctionName("PullRequestStateUtility_CreateOrUpdatePullRequestState")]
        public async Task CreateOrUpdatePullRequestState(
            [ActivityTrigger] PullRequestState state,
            [Table("PullRequestState")] IAsyncCollector<PullRequestState> collector,
            ILogger log)
        {

            await collector.AddAsync(state);
        }
    }
}
