using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using JiraDevOpsIntegrationFunctions.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace JiraDevOpsIntegrationFunctions
{
    public static class GetPRsAndRepos
    {
        [FunctionName("GetPRsAndRepos")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Table("PRIssueMapping")] CloudTable table,
            [Table("IssueRepoMapping")] CloudTable table2,
            [Table("GroupPrefix")] CloudTable table3,
            ILogger log)
        {
            var PRs = new List<string>();
            var Repos = new List<RepoInfo>();
            var PullInfo = new List<PR>();
            dynamic data = JObject.Parse(await new StreamReader(req.Body).ReadToEndAsync());
            string RowKey = data.key;
            string prefix = "";
            string urlName = "";
            HttpClient client = new HttpClient();
            string url = "";
            var byteArray = Encoding.ASCII.GetBytes($":{Environment.GetEnvironmentVariable("AzureDevOps_Setting", EnvironmentVariableTarget.Process)}");

            TableQuery<PRIssueMapping> rangeQuery = new TableQuery<PRIssueMapping>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, RowKey));
            foreach (PRIssueMapping issue in await table.ExecuteQuerySegmentedAsync(rangeQuery, null))
            {
                string[] issueSplit = issue.PartitionKey.Split("|");
                prefix = issueSplit[0];
                PRs.Add(issueSplit[1]);
            }
            string[] PRArray = PRs.ToArray();

            TableQuery<GroupPrefix> rangeQuery3 = new TableQuery<GroupPrefix>().Where(TableQuery.GenerateFilterCondition("Prefix", QueryComparisons.Equal, prefix));
            foreach (GroupPrefix repo in await table3.ExecuteQuerySegmentedAsync(rangeQuery3, null))
            {
                string[] urlSplit = repo.PartitionKey.Split(" ");
                urlName = urlSplit[1];
            }

            string PartitionKey = $"{prefix}|{RowKey}";
            TableQuery<PRRepoMapping> rangeQuery2 = new TableQuery<PRRepoMapping>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey));
            foreach (PRRepoMapping repo in await table2.ExecuteQuerySegmentedAsync(rangeQuery2, null))
            {
                url = $"https://dev.azure.com/{urlName}/_apis/git/repositories/{repo.RowKey}?api-version=4.1";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                HttpResponseMessage response = await client.SendAsync(request);
                dynamic repoJSON = JObject.Parse(await response.Content.ReadAsStringAsync());
                string name = repoJSON.name;
                Repos.Add(new RepoInfo() {status = repo.MergeStatus, repoName = name});
            }
            RepoInfo[] RepoArray = Repos.ToArray();


            foreach (string PR in PRArray)
            {
                url = $"https://dev.azure.com/{urlName}/_apis/git/pullrequests/{PR}?api-version=5.1";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);                
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                HttpResponseMessage response = await client.SendAsync(request);
                dynamic titleJSON = JObject.Parse(await response.Content.ReadAsStringAsync());
                string title = titleJSON.repository.name;
                PullInfo.Add(new PR() { ID = PR, name = urlName, repoTitle = title });
            }
            PR[] pullInfo = PullInfo.ToArray();          

            return new OkObjectResult(new PRAndRepoResponse() { PullRequests = pullInfo, Repos = RepoArray });
        }
    }
}
