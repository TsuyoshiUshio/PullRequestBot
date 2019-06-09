using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Xunit;
using PullRequestBot;
using PullRequestLibrary;
using PullRequestLibrary.Provider.GitHub;

namespace PullRequestBot.Test
{
    public class StartupTest
    {
        private void Setup(string gitHubRepositoryOwner,
            string gitHubRepositoryName,
            string gitHubPat,
            string sonarCloudPat)
        {
            // GitHub setup
            Environment.SetEnvironmentVariable("GitHubRepositoryOwner", gitHubRepositoryOwner);
            Environment.SetEnvironmentVariable("GitHubRepositoryName", gitHubRepositoryName);
            Environment.SetEnvironmentVariable("GitHubPAT", gitHubPat);
            // SonarCloud setup
            Environment.SetEnvironmentVariable("SonarCloudPAT", sonarCloudPat);
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
                "qux");

            var startup = new Startup();
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
                sonarCloudPat);

            var expectedAuthorizationParameter = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes(
                    string.Format("{0}:{1}", sonarCloudPat, "")));
            var startup = new Startup();
            var mock = new WebJobsBuilderMock(new ServiceCollection());
            startup.Configure(mock);
            var provider = mock.Services.BuildServiceProvider();
            var iContext = provider.GetService<IRestClientContext>();
            var context = (RestClientContext)iContext;
            Assert.Equal(expectedAuthorizationParameter, context.client.DefaultRequestHeaders.Authorization.Parameter);
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
