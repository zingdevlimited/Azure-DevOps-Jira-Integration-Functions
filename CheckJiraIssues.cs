using JiraDevOpsIntegrationFunctions.Helpers;
using JiraDevOpsIntegrationFunctions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using ProjectFunctions.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JiraDevOpsIntegrationFunctions
{
    public static class CheckJiraIssues
    {
        [FunctionName("CheckJiraIssues")]
        public async static Task Run([ServiceBusTrigger(Constants.ServiceBus, Constants.JiraIssuesTriggerName, Connection = Constants.ServiceBusConnectionName)]PRInfo info, 
            [Table("PRIssueMapping")] CloudTable issueMappingTable, 
            ILogger log)
        {
            string PartitionKey = $"{info.Prefix}|{info.PullRequestID}";
            int records = 0;
            TableQuery<PRIssueMapping> rangeQuery = new TableQuery<PRIssueMapping>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey));
            foreach (PRIssueMapping issue in await issueMappingTable.ExecuteQuerySegmentedAsync(rangeQuery, null))
            {
                records++;
            }
            HttpClient client = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes($":{Environment.GetEnvironmentVariable(Constants.AzureDevOpsConnectionName, EnvironmentVariableTarget.Process)}");
            string statusURL = $"{info.BaseURL}/_apis/git/repositories/{info.RepoID}/pullRequests/{info.PullRequestID}/statuses/statuses?api-version=5.1-preview.1";
            Environment.SetEnvironmentVariable(Constants.BaseUrlConnectionName, info.BaseURL);
            HttpRequestMessage statusChange = new HttpRequestMessage(HttpMethod.Post, statusURL);
            statusChange.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(Constants.AzureDevOpsAuthenticationHeaderInstruction, Convert.ToBase64String(byteArray));
            JiraIssueModel obj;
            if (records == 0)
            {
                obj = CreateIssueModel("pending", "Click here to select Issues", $"{Environment.GetEnvironmentVariable(Constants.SpaUrlConnectionName, EnvironmentVariableTarget.Process)}/{info.Prefix}/{info.PullRequestID}/{info.Token}");
            }
            else
            {
                obj = CreateIssueModel("succeeded", $"Linked to {records} issues", $"{Environment.GetEnvironmentVariable(Constants.SpaUrlConnectionName, EnvironmentVariableTarget.Process)}/{info.Prefix}/{info.PullRequestID}/{info.Token}");
            }
            string json = JsonConvert.SerializeObject(obj);
            statusChange.Content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage statusResponse = await client.SendAsync(statusChange);
            if (!statusResponse.IsSuccessStatusCode)
            {
                log.LogError("Error occured when fetching CheckJiraIssues repo info!");
            }            
        }

        private static JiraIssueModel CreateIssueModel(string state, string description, string url)
        {
            return new JiraIssueModel()
            {
                State = state,
                Description = description,
                Context = new Context()
                {
                    Name = "JiraIssues"
                },
                TargetUrl = url
            };            
        }
    }
}
