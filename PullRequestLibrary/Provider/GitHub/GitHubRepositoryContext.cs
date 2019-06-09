﻿using Microsoft.VisualStudio.Services.Account;
using System;
using System.Collections.Generic;
using System.Text;

namespace PullRequestLibrary.Provider.GitHub
{
    public interface IGitHubRepositoryContext
    {
        string Name { get; set; }
        string Owner { get; set; }
    }

    public class GitHubRepositoryContext : IGitHubRepositoryContext
    {
        public string Owner { get; set; }
        public string Name { get; set; }
    }
}
