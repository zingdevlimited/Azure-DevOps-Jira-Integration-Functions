using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using JiraDevOpsIntegrationFunctions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using ProjectFunctions.Models;

namespace JiraDevOpsIntegrationFunctions
{
    public static class CheckJiraIssues
    {
        [FunctionName("CheckJiraIssues")]
        public async static Task Run([ServiceBusTrigger("prupdated", "CheckJiraIssues", Connection = "AzureWebJobsServiceBus")]PRInfo info, [Table("PRIssueMapping")] CloudTable table, ILogger log)
        {
            string PartitionKey = $"{info.Prefix}|{info.PullRequestID}";
            int records = 0;
            TableQuery<PRIssueMapping> rangeQuery = new TableQuery<PRIssueMapping>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey));
            foreach (PRIssueMapping issue in await table.ExecuteQuerySegmentedAsync(rangeQuery, null))
            {
                records++;
            }
            HttpClient client = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes($":{Environment.GetEnvironmentVariable("AzureDevOps_Setting", EnvironmentVariableTarget.Process)}");
            string statusURL = $"{info.BaseURL}/_apis/git/repositories/{info.RepoID}/pullRequests/{info.PullRequestID}/statuses/statuses?api-version=5.1-preview.1";
            HttpRequestMessage statusChange = new HttpRequestMessage(HttpMethod.Post, statusURL);
            statusChange.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            JiraIssueModel obj;
            if (records == 0)
            {
                obj = new JiraIssueModel()
                {
                    state = "pending",
                    description = "Click here to select Issues",
                    context = new Context()
                    {
                        name = "JiraIssues"
                    },
                    targetUrl = $"{Environment.GetEnvironmentVariable("SPAUrl", EnvironmentVariableTarget.Process)}/{info.Prefix}/{info.PullRequestID}/{info.Token}"
                };
            }
            else
            {
                obj = new JiraIssueModel()
                {
                    state = "succeeded",
                    description = $"Linked to {records} issues" ,
                    context = new Context()
                    {
                        name = "JiraIssues"
                    },
                    targetUrl = $"{Environment.GetEnvironmentVariable("SPAUrl", EnvironmentVariableTarget.Process)}/{info.Prefix}/{info.PullRequestID}/{info.Token}"
                };
            }
            dynamic json = JsonConvert.SerializeObject(obj);
            statusChange.Content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage statusResponse = await client.SendAsync(statusChange);
            if (!statusResponse.IsSuccessStatusCode)
            {
                log.LogError("Error!");
            }            
        }
    }
}
