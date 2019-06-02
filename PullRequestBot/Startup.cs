using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using PullRequestBot;
using PullRequestLibrary;
using System;

[assembly: WebJobsStartupAttribute(typeof(Startup))]
namespace PullRequestBot
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            VssConnection connection = CreateVssConnection();
            builder.Services.AddSingleton<GitHttpClientBase>(connection.GetClient<GitHttpClient>());
            builder.Services.AddSingleton<WorkItemTrackingHttpClientBase>(connection.GetClient<WorkItemTrackingHttpClient>());
            builder.Services.AddSingleton<IPullRequestRepository>();
            builder.Services.AddSingleton<IWorkItemRepository>();
        }

        private VssConnection CreateVssConnection()
        {
            string BaseURL = Environment.GetEnvironmentVariable("BaseURL");
            string PAT = Environment.GetEnvironmentVariable("PAT");
            var uri = new Uri(BaseURL);
            var organizationURL = BaseURL.Substring(0, BaseURL.LastIndexOf('/'));

            var connection = new VssConnection(new Uri(organizationURL), new VssBasicCredential(string.Empty, PAT));
            return connection;
        }
    }
}
