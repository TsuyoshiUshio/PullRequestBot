using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PullRequestBot;
using PullRequestLibrary;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

[assembly: WebJobsStartupAttribute(typeof(Startup))]
namespace PullRequestBot
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.AddSingleton<HttpClient>(CreateHttpClientForAzureDevOpsRest());
            builder.Services.AddSingleton<IRestClientContext>();
            builder.Services.AddSingleton<IPullRequestRepository>();
            builder.Services.AddSingleton<IWorkItemRepository>();
        }

        private HttpClient CreateHttpClientForAzureDevOpsRest()
        {
            var client = new HttpClient();

            string BaseURL = Environment.GetEnvironmentVariable("BaseURL");
            string PAT = Environment.GetEnvironmentVariable("PAT");


            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    System.Text.ASCIIEncoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", "", PAT))));

            var uri = new Uri(BaseURL);
            client.BaseAddress = new Uri($"https://{uri.Host}");
            return client;
        }
    }
}
