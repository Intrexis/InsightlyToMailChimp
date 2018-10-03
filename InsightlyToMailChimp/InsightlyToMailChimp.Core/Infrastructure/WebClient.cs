using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace InsightlyToMailChimp.Core.Infrastructure
{
    public static class WebClient
    {
        public static async Task<HttpResponseMessage> GetRequest(string url, Dictionary<string, string> headers = null)
        {
            var client = new HttpClient();

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            return await client.GetAsync(url);
        }
    }
}