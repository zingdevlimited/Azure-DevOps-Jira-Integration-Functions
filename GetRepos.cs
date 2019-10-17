using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JiraDevOpsIntegrationFunctions.Helpers;
using JiraDevOpsIntegrationFunctions.Models;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace JiraDevOpsIntegrationFunctions
{
    public static class GetRepos
    {
        [FunctionName("GetRepos")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] ValidatePRInfoRequest req,
            [Table(Constants.PullRequestTable, "{groupId}", "{pullRequestId}")] PRDetail Match,
            [Table(Constants.PrefixTable)] CloudTable GroupPrefixTable, ILogger log)
        {
            TableQuery<GroupPrefix> GetProjectName = new TableQuery<GroupPrefix>().Where(TableQuery.GenerateFilterCondition("Prefix", QueryComparisons.Equal, req.GroupId));
            string ProjectName = (await GroupPrefixTable.ExecuteQuerySegmentedAsync(GetProjectName, null)).Results[0].PartitionKey.Split(" ")[1];

            List<Repo> repos = new List<Repo>();
            if (Match == null)
                return new NotFoundResult();

            string url = $"https://dev.azure.com/{ProjectName}/_apis/git/repositories?api-version=4.1";
            HttpClient client = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes($":{Environment.GetEnvironmentVariable(Constants.AzureDevOpsConnectionName, EnvironmentVariableTarget.Process)}");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            HttpResponseMessage response = await client.SendAsync(request);
            dynamic JSON = JObject.Parse(await response.Content.ReadAsStringAsync())["value"];
            dynamic x = JSON[0];
            dynamic y = x.id;
            foreach(dynamic item in JSON)
            {
                string name = item.name;
                string id = item.id;
                repos.Add(new Repo() { name = name, id = id});
            }
            return new OkObjectResult(repos.ToArray());
        }
    }
}
