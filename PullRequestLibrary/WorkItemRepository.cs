using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using PullRequestLibrary.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PullRequestLibrary
{
    public interface IWorkItemRepository
    {
        Task<WorkItem> CreateWorkItem(WorkItemSource workItem);
    }

    public class WorkItemRepository : IWorkItemRepository
    {
        private WorkItemTrackingHttpClientBase client;

        public WorkItemRepository(WorkItemTrackingHttpClientBase client)
        {
            this.client = client;
        }


        public Task<WorkItem> CreateWorkItem(WorkItemSource workItem)
        { 
            var project = "DevSecOps";
            var type = "Bug";

            var document = workItem.ToJsonPatchDocument();
            return client.CreateWorkItemAsync(document, project, type);
        }
    }
}
