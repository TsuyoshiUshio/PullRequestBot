using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using PullRequestBot;
using PullRequestLibrary.Provider.GitHub;
using System;
using System.Net.Http;
using Octokit;
using PullRequestLibrary;

[assembly: WebJobsStartupAttribute(typeof(Startup))]
namespace PullRequestBot
{
    public class Startup : IWebJobsStartup
    {
        private static readonly string GitHubRepositoryOwner = Environment.GetEnvironmentVariable("GitHubRepositoryOwner");
        private static readonly string GitHubRepositoryName = Environment.GetEnvironmentVariable("GitHubRepositoryName");
        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.AddSingleton<IGitHubRepositoryContext>(GetGitHubRepositoryContext());
            builder.Services.AddSingleton<IGitHubClient>(GetGitHubClient());
            builder.Services.AddSingleton<IRestClientContext>(GetRestClientContext());
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
            var tokenAuth = new Credentials(GitHubRepositoryOwner, Environment.GetEnvironmentVariable("GitHubPAT"));
            client.Credentials = tokenAuth;
            return client;
        }

        private IRestClientContext GetRestClientContext()
        {
            var sonarCloudPat = Environment.GetEnvironmentVariable("SonarCloudPAT");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", sonarCloudPat, ""))));
            return new RestClientContext(client);
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
