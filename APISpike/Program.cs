using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

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

            ExecAsync().GetAwaiter().GetResult();
            Console.ReadLine();

        }

        public static async Task ExecAsync()
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
