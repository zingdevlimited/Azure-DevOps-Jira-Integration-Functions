using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ProjectFunctions.Models;

namespace JiraDevOpsIntegrationFunctions
{
    public static class CheckJiraIssues
    {
        [FunctionName("CheckJiraIssues")]
        public async static Task Run([ServiceBusTrigger("prupdated", "CheckJiraIssues", Connection = "AzureWebJobsServiceBus")]PRInfo info, ILogger log)
        {
            bool response = true;
            bool response2 = false;
            HttpClient client = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes($":{Environment.GetEnvironmentVariable("AzureDevOps_Setting", EnvironmentVariableTarget.Process)}");
            if (response)
            {
                string statusURL = $"{info.BaseURL}/_apis/git/repositories/{info.RepoID}/pullRequests/{info.PullRequestID}/statuses/statuses?api-version=5.1-preview.1";
                HttpRequestMessage statusChange = new HttpRequestMessage(HttpMethod.Post, statusURL);
                statusChange.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                string json;
                if (response2)
                {
                    json = @"{ ""state"": ""failed"", ""description"": ""Click here to select Issues"", ""context"": { ""name"": ""JiraIssues"" }, ""targetUrl"": ""https://www.zing.dev""}";
                }
                else
                {
                    json = @"{ ""state"": ""succeeded"", ""description"": ""Linked to X issues"", ""context"": { ""name"": ""JiraIssues"" }, ""targetUrl"": ""https://www.zing.dev""}";
                }
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                statusChange.Content = content;
                HttpResponseMessage statusResponse = await client.SendAsync(statusChange);
                if (!statusResponse.IsSuccessStatusCode)
                {
                    log.LogError("Error!");
                }
            }
        }
    }
}
