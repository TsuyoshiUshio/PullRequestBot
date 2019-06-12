using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Moq;
using Octokit;
using Xunit;
using PullRequestBot;
using PullRequestLibrary;
using PullRequestLibrary.Provider.GitHub;
using PullRequestLibrary.Provider.SonarCloud;
using PullRequestLibrary.Command;
using PullRequestLibrary.Provider.AzureDevOps;

namespace PullRequestBot.Test
{
    public class StartupTest
    {
        private void Setup(string gitHubRepositoryOwner,
            string gitHubRepositoryName,
            string gitHubPat,
            string sonarCloudPat,
            string azureDevOpsBaseUrl,
            string azureDevOpsPat
            )
        {
            // GitHub setup
            Environment.SetEnvironmentVariable("GitHubRepositoryOwner", gitHubRepositoryOwner);
            Environment.SetEnvironmentVariable("GitHubRepositoryName", gitHubRepositoryName);
            Environment.SetEnvironmentVariable("GitHubPAT", gitHubPat);
            // SonarCloud setup
            Environment.SetEnvironmentVariable("SonarCloudPAT", sonarCloudPat);
            // AzureDevOps setup
            Environment.SetEnvironmentVariable("AzureDevOpsBaseURL", azureDevOpsBaseUrl);
            Environment.SetEnvironmentVariable("AzureDevOpsPAT", azureDevOpsPat);

        }

        [Fact]
        public void GitHubSettingsTest()
        {
            const string expectedGitHubRepositoryOwner = "foo";
            const string expectedGitHubRepositoryName = "bar";
            const string expectedGitHubPat = "baz";
            Setup(expectedGitHubRepositoryOwner, 
                expectedGitHubRepositoryName,
                expectedGitHubPat,
                "qux",
                "https://dev.azure.com/quux/foobar",
                "corge");

            var startup = new StartupMock();
            var mock = new WebJobsBuilderMock(new ServiceCollection());

            startup.Configure(mock);
            var provider = mock.Services.BuildServiceProvider();
            var context = provider.GetService<IGitHubRepositoryContext>();
            var iClient = provider.GetService<IGitHubClient>();
            Assert.Equal(expectedGitHubRepositoryOwner, context.Owner);
            Assert.Equal(expectedGitHubRepositoryName, context.Name);
            GitHubClient client = (GitHubClient) iClient;
            Assert.Equal(expectedGitHubRepositoryOwner,client.Credentials.Login);
            Assert.Equal(expectedGitHubPat, client.Credentials.Password);
        }

        [Fact]
        public void SonarCloudSettingsTest()
        {
            const string sonarCloudPat = "foo";
            Setup("bar",
                "baz",
                "qux",
                sonarCloudPat,
                "https://dev.azure.com/quux/foobar",
                "corge");

            var expectedAuthorizationParameter = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes(
                    string.Format("{0}:{1}", sonarCloudPat, "")));
            var startup = new StartupMock();
            var mock = new WebJobsBuilderMock(new ServiceCollection());
            startup.Configure(mock);
            var provider = mock.Services.BuildServiceProvider();
            var iContext = provider.GetService<IRestClientContext>();
            var context = (RestClientContext)iContext;
            Assert.Equal(expectedAuthorizationParameter, context.client.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void SetUpRepositories()
        {
            Setup("foo", "bar","baz","qux", "https://dev.azure.com/quux/foobar", "corge");
            var startup = new StartupMock();
            var mock = new WebJobsBuilderMock(new ServiceCollection());
            startup.Configure(mock);
            var provider = mock.Services.BuildServiceProvider();

            var gitHubRepository = provider.GetService<IGitHubRepository>();
            Assert.NotNull(gitHubRepository);
            Assert.NotNull(((GitHubRepository)gitHubRepository).client);

            var sonarCloudRepository = provider.GetService<ISonarCloudRepository>();
            Assert.NotNull(sonarCloudRepository);
            Assert.NotNull(((SonarCloudRepository)sonarCloudRepository).context);

            var ciHookService = provider.GetService<ICIHookService>();
            Assert.NotNull(ciHookService);
        }

        [Fact]
        public void SetUpWorkItemRepository()
        {
            Setup("foo", "bar", "baz", "qux", "https://dev.azure.com/quux/foobar", "corge");
            var startup = new StartupMock();
            var mock = new WebJobsBuilderMock(new ServiceCollection());
            startup.Configure(mock);
            var provider = mock.Services.BuildServiceProvider();
            var workItemRepository = provider.GetService<IWorkItemRepository>();
            Assert.NotNull(workItemRepository);
        }

        [Fact]
        public void SetupCommandHookService()
        {
            Setup("foo", "bar", "baz", "qux", "https://dev.azure.com/quux/foobar", "corge");
            var startup = new StartupMock();
            var mock = new WebJobsBuilderMock(new ServiceCollection());
            startup.Configure(mock);
            var provider = mock.Services.BuildServiceProvider();
            var commandContext = provider.GetService<ICommandContext>();
            Assert.NotNull(commandContext);


        }

        public class StartupMock : Startup
        {
            public VssConnection Connection { get; set; }
            internal override WorkItemTrackingHttpClientBase GetWorkItemTrackingHttpClient(VssConnection connection)
            {
                Connection = connection;

                var clientMock = new Mock<WorkItemTrackingHttpClientBase>(MockBehavior.Strict, new Uri("https://foo.bar"), new VssCredentials());
                return clientMock.Object;
            }
        }

        public class WebJobsBuilderMock : IWebJobsBuilder
        { 
            public IServiceCollection Services { get; }
            public WebJobsBuilderMock(ServiceCollection serviceCollection)
            {
                Services = serviceCollection;
            }
        }

    }
}
