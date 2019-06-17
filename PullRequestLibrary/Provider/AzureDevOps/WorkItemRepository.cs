using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System.Threading.Tasks;
using PullRequestLibrary.Model;

namespace PullRequestLibrary.Provider.AzureDevOps
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
