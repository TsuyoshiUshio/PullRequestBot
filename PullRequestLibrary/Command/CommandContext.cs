using System;
using System.Collections.Generic;
using System.Text;
using PullRequestLibrary.Generated.GitHub.PRCommentCreated;
using PullRequestLibrary.Provider.AzureDevOps;
using PullRequestLibrary.Provider.GitHub;
using PullRequestLibrary.Provider.SonarCloud;

namespace PullRequestLibrary.Command
{
    public interface ICommandContext
    {
        IGitHubRepository GitHubRepository { get; }
        IGitHubRepositoryContext RepositoryContext { get; }
        ISonarCloudRepository SonarCloudRepository { get; }
        IWorkItemRepository WorkItemRepository { get; }
        PRCommentCreated PRCommentCreated { get; set; }
    }

    public class CommandContext : ICommandContext
    {
        // Dependency Injection
        public IGitHubRepository GitHubRepository { get; }
        public IGitHubRepositoryContext RepositoryContext { get; }
        public ISonarCloudRepository SonarCloudRepository { get; }
        public IWorkItemRepository WorkItemRepository { get; }

        // Framework pass the value
        public PRCommentCreated PRCommentCreated { get; set; }

        public CommandContext(IGitHubRepository gitHubRepository,
            IGitHubRepositoryContext repositoryContext,
            ISonarCloudRepository sonarCloudRepository,
            IWorkItemRepository workItemRepository)
        {
            this.GitHubRepository = gitHubRepository;
            this.RepositoryContext = repositoryContext;
            this.SonarCloudRepository = sonarCloudRepository;
            this.WorkItemRepository = workItemRepository;
        }
    }
}
