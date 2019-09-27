using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ProjectFunctions.Models;

namespace ProjectFunctions
{
    public static class CheckStaleBranch
    {
        [FunctionName("CheckStaleBranch")]
        public async static Task Run([ServiceBusTrigger("prupdated", "CheckStaleBranch", Connection = "AzureWebJobsServiceBus")]PRInfo info, ILogger log)
        {
            string url = $"{info.BaseURL}_apis/git/repositories/{info.RepoID}/stats/branches?name={info.Source}&baseVersionDescriptor.version={info.Target}&api-version=5.1";
            HttpClient client = new HttpClient();
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
            var byteArray = Encoding.ASCII.GetBytes($":{Environment.GetEnvironmentVariable("AzureDevOps_Setting", EnvironmentVariableTarget.Process)}");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",Convert.ToBase64String(byteArray));
            HttpResponseMessage response = await client.SendAsync(req);
            if (response.IsSuccessStatusCode)
            {
                JObject data = JObject.Parse(await response.Content.ReadAsStringAsync());
                string statusURL = $"{info.BaseURL}/_apis/git/repositories/{info.RepoID}/pullRequests/{info.PullRequestID}/statuses/statuses?api-version=5.1-preview.1";
                HttpRequestMessage statusChange = new HttpRequestMessage(HttpMethod.Post, statusURL);
                statusChange.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                string json;
                if (Int32.Parse(data["behindCount"].ToString()) > 0)
                {
                    json = @"{ ""state"": ""failed"", ""description"": ""Behind branch"", ""context"": { ""name"": ""staleBranch"" }}";
                }
                else
                {
                    json = @"{ ""state"": ""succeeded"", ""description"": ""Up to date"", ""context"": { ""name"": ""staleBranch"" }}";
                }
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                statusChange.Content = content;
                HttpResponseMessage statusResponse = await client.SendAsync(statusChange);
                if (!statusResponse.IsSuccessStatusCode)
                {
                    log.LogError("Error!");
                }
            }
            else
            {
                log.LogError("Error");
            }
        }
    }
}
