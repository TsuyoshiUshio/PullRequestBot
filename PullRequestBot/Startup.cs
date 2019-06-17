using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using PullRequestBot;
using PullRequestLibrary.Provider.GitHub;
using System;
using System.Net.Http;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Octokit;
using PullRequestLibrary;
using PullRequestLibrary.Provider.AzureDevOps;
using PullRequestLibrary.Provider.SonarCloud;

[assembly: WebJobsStartupAttribute(typeof(Startup))]
namespace PullRequestBot
{
    public class Startup : IWebJobsStartup
    {
        private const string GitHubRepositoryOwnerSetting = "GitHubRepositoryOwner";
        private const string GitHubRepositoryNameSetting = "GitHubRepositoryName";
        private const string GitHubPatSetting = "GitHubPAT";
        private const string SonarCloudPatSetting = "SonarCloudPAT";
        private const string AzureDevOpsBaseURL = "AzureDevOpsBaseURL";
        private const string AzureDevOpsPAT = "AzureDevOpsPAT";

        private static readonly string GitHubRepositoryOwner = Environment.GetEnvironmentVariable(GitHubRepositoryOwnerSetting);
        private static readonly string GitHubRepositoryName = Environment.GetEnvironmentVariable(GitHubRepositoryNameSetting);

        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.AddSingleton<IGitHubRepositoryContext>(GetGitHubRepositoryContext());
            builder.Services.AddSingleton<IGitHubClient>(GetGitHubClient());
            builder.Services.AddSingleton<IRestClientContext>(GetRestClientContext());
            builder.Services.AddSingleton<IGitHubRepository, GitHubRepository>();
            builder.Services.AddSingleton<ISonarCloudRepository, SonarCloudRepository>();


            VssConnection connection = CreateVssConnection();
            builder.Services.AddSingleton<IWorkItemRepository>(
                new WorkItemRepository(GetWorkItemTrackingHttpClient(connection)));

        }

        private IGitHubRepositoryContext GetGitHubRepositoryContext()
        {
            return new GitHubRepositoryContext()
            {
                Owner = GitHubRepositoryOwner,
                Name  = GitHubRepositoryName
            };
        }

        private IGitHubClient GetGitHubClient()
        {
            var client = new GitHubClient(new ProductHeaderValue("PullRequestBot"));
            var tokenAuth = new Credentials(GitHubRepositoryOwner, Environment.GetEnvironmentVariable(GitHubPatSetting));
            client.Credentials = tokenAuth;
            return client;
        }

        private IRestClientContext GetRestClientContext()
        {
            var sonarCloudPat = Environment.GetEnvironmentVariable(SonarCloudPatSetting);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", sonarCloudPat, ""))));
            return new RestClientContext(client);
        }

        internal virtual WorkItemTrackingHttpClientBase GetWorkItemTrackingHttpClient(VssConnection connection)
        {
            
            return connection.GetClient<WorkItemTrackingHttpClient>();
        }


        // Connection Setting for Azure DevOps
        // Currently not used. 
        private VssConnection CreateVssConnection()
        {
            string BaseURL = Environment.GetEnvironmentVariable(AzureDevOpsBaseURL);
            string PAT = Environment.GetEnvironmentVariable(AzureDevOpsPAT);
            var uri = new Uri(BaseURL);
            var organizationURL = BaseURL.Substring(0, BaseURL.LastIndexOf('/'));

            var connection = new VssConnection(new Uri(organizationURL), new VssBasicCredential(string.Empty, PAT));
            return connection;
        }
    }
}
