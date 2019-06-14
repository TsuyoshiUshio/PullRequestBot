using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.WebJobs;
using PullRequestLibrary.Model;

namespace PullRequestBot.Entity
{
    public class PullRequestEntity
    {
        [FunctionName(nameof(PullRequestEntity))]
        public void EntryPoint(
            [EntityTrigger] IDurableEntityContext ctx)
        {
            var current = ctx.GetState<PullRequestStateContext>();
            var input = ctx.GetInput<PullRequestStateContext>();

            switch (ctx.OperationName)
            {
                case "get":
                    break;
                case "update":
                    current = input;
                    break;
            }
            ctx.SetState(current);
        }

    }

}
