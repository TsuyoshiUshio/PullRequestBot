using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PullRequestLibrary
{
    public interface IRestClientContext
    {
        Task<T> GetAsync<T>(string requestUri);
        Task<T> PostAsync<K, T>(string requestUri, K payload);
    }

    public class RestClientContext : IRestClientContext
    {
        internal HttpClient client; // internal for testability

        public RestClientContext(HttpClient client)
        {
            this.client = client;
        }

        public async Task<T> GetAsync<T>(string requestUri)
        {
            using (HttpResponseMessage response = await client.GetAsync(requestUri))
            {
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
        }

        public async Task<T> PostAsync<K, T>(string requestUri, K payload)
        {
            var data = JsonConvert.SerializeObject(payload);
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await client.PostAsync(requestUri, content))
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
        }
    }
}
