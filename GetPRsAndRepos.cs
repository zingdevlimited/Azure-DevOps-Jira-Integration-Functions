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
using JiraDevOpsIntegrationFunctions.Helpers;
using System.IdentityModel.Tokens.Jwt;

namespace JiraDevOpsIntegrationFunctions
{
    public static class GetPRsAndRepos
    {
        [FunctionName(nameof(GetPRsAndRepos))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Table(Constants.IssueMappingTable)] CloudTable IssueMappingTable,
            [Table(Constants.RepoMappingTable)] CloudTable RepoMappingTable,
            [Table(Constants.PrefixTable)] CloudTable GroupPrefixTable,
            ILogger log)
        {
            List<RepoInfo> Repos = new List<RepoInfo>();
            List<PR> PRs = new List<PR>();
            dynamic Data = JObject.Parse(await new StreamReader(req.Body).ReadToEndAsync());
            try
            {
                string Jwt = Data.jwt;
                log.LogError(Jwt);
                JwtSecurityToken x = new JwtSecurityToken(Jwt);
                log.LogError("Encoded PayLoad: " + x.EncodedPayload);
                if (Environment.GetEnvironmentVariable(Constants.JwtClientName, EnvironmentVariableTarget.Process) != new JwtSecurityToken(Jwt).Payload["iss"].ToString())
                {
                    return new BadRequestResult();
                }
            }
            catch(Exception e)
            {
                log.LogError("Error occured: Fetching data from table storage");
                return new BadRequestResult();
            }

            string ProjectName = "";

            TableQuery<GroupPrefix> GetProjectName = new TableQuery<GroupPrefix>().Where(TableQuery.GenerateFilterCondition("Prefix", QueryComparisons.Equal, Data.key.ToString().Split("-")[0]));
            ProjectName = (await GroupPrefixTable.ExecuteQuerySegmentedAsync(GetProjectName, null)).Results[0].PartitionKey.Split(" ")[1];

            TableQuery<PRIssueMapping> GetPRs = new TableQuery<PRIssueMapping>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, Data.key.ToString()));
            foreach (PRIssueMapping issue in await IssueMappingTable.ExecuteQuerySegmentedAsync(GetPRs, null))
            {
                dynamic name = await GetAzureDevOpsInfo($"https://dev.azure.com/{ProjectName}/_apis/git/pullrequests/{issue.PartitionKey.Split("|")[1]}?api-version=5.1", "PR");
                PRs.Add(new PR() { Id = issue.PartitionKey.Split("|")[1] , Name = ProjectName, RepoTitle = name });
            }

            TableQuery<PRRepoMapping> GetRepos = new TableQuery<PRRepoMapping>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{Data.key.ToString().Split("-")[0]}|{Data.key.ToString()}"));
            foreach (PRRepoMapping repo in await RepoMappingTable.ExecuteQuerySegmentedAsync(GetRepos, null))
            {
                dynamic name = await GetAzureDevOpsInfo($"https://dev.azure.com/{ProjectName}/_apis/git/repositories/{repo.RowKey}?api-version=4.1", "Repo");
                Repos.Add(new RepoInfo() {Status = repo.MergeStatus, RepoName = name});
            }
            
            return new OkObjectResult(new PRAndRepoResponse() { PullRequests = PRs.ToArray(), Repos = Repos.ToArray() });
        }
        public static async Task<dynamic> GetAzureDevOpsInfo(string url, string type)
        {
            HttpClient client = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes($":{Environment.GetEnvironmentVariable(Constants.AzureDevOpsConnectionName, EnvironmentVariableTarget.Process)}");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            HttpResponseMessage response = await client.SendAsync(request);
            dynamic JSON = JObject.Parse(await response.Content.ReadAsStringAsync());
            string name = type == "PR" ? JSON.repository.name : JSON.name;
            return name;
        }
    }
}
