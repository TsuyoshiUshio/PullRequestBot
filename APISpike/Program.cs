using Microsoft.Extensions.Configuration;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace APISpike
{
    class Program
    {
        private static IConfiguration Configuration { get; set; }
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            Configuration = builder.Build();

            //ExecWithRestAsync().GetAwaiter().GetResult();
            ExecWithClientLibrary().GetAwaiter().GetResult();
            Console.ReadLine();

        }

        public static async Task ExecWithClientLibrary()
        {
            var BaseURL = Configuration["BaseURL"];
            var PAT = Configuration["AzureDevOpsPAT"];
            var RepositoryId = Configuration["RepositoryId"];
            var uri = new Uri(BaseURL);
            var organizationURL = BaseURL.Substring(0, BaseURL.LastIndexOf('/'));
      
            VssConnection connection = new VssConnection(new Uri(organizationURL), new VssBasicCredential(string.Empty, PAT));
            // List PR thread. 

            GitHttpClient client = connection.GetClient<GitHttpClient>();
            var result = await client.GetPullRequestAsync(RepositoryId, 53);
            var thtreads = await client.GetThreadsAsync(RepositoryId, 53);
   
            var clientMock = new Mock<GitHttpClientBase>(MockBehavior.Strict, new Uri("https://foo.bar"), new VssCredentials());
            clientMock.Setup(p =>
             p.GetThreadsAsync(RepositoryId, 53, It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<GitPullRequestCommentThread>()).Verifiable();
            var r = clientMock.Object.GetThreadsAsync(RepositoryId, 53);
            clientMock.Verify();
       }

        public static async Task ExecWithRestAsync()
        {
            var BaseURL = Configuration["BaseURL"];
            var PAT = Configuration["AzureDevOpsPAT"];

            // Spike solution for sending request to the Azure DevOps RestAPI.

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", "", PAT))));

                    var uri = new Uri(BaseURL);
                    client.BaseAddress = new Uri($"https://{uri.Host}");

                    using (HttpResponseMessage response = await client.GetAsync(
                        $"{uri.LocalPath}/_apis/git/pullrequests/53?api-version=5.0"))
                    {
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(responseBody);
                    }

                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }      
        }
    }
}
