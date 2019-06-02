using PullRequestLibrary.Generated.WorkItemCreated;
using PullRequestLibrary.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PullRequestLibrary
{
    public interface IWorkItemRepository
    {
        Task<WorkItemCreated> CreateWorkItem(WorkItem workItem);
    }

    public class WorkItemRepository : IWorkItemRepository
    {
        private IRestClientContext context;

        public WorkItemRepository(IRestClientContext context)
        {
            this.context = context;
        }

        public Task<WorkItemCreated> CreateWorkItem(WorkItem workItem)
        {
            var organization = "csedevops";
            var project = "DevSecOps";
            var type = "Bug";
            return context.PostAsync<List<WorkItemField>, WorkItemCreated>($"https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/${type}?api-version=5.0", workItem.ToWorkItemFields());
        }
    }
}
