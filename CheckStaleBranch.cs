using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JiraDevOpsIntegrationFunctions.Helpers;
using JiraDevOpsIntegrationFunctions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectFunctions.Models;

namespace ProjectFunctions
{
    public static class CheckStaleBranch
    {
        [FunctionName("CheckStaleBranch")]
        public async static Task Run([ServiceBusTrigger(Constants.ServiceBus, Constants.StaleBranchTriggerName, Connection = Constants.ServiceBusConnectionName)]PRInfo info, ILogger log)
        {
            string url = $"{info.BaseURL}_apis/git/repositories/{info.RepoID}/stats/branches?name={info.Source}&baseVersionDescriptor.version={info.Target}&api-version=5.1";
            HttpClient client = new HttpClient();
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, url);
            var byteArray = Encoding.ASCII.GetBytes($":{Environment.GetEnvironmentVariable(Constants.AzureDevOpsConnectionName, EnvironmentVariableTarget.Process)}");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(Constants.AzureDevOpsAuthenticationHeaderInstruction,Convert.ToBase64String(byteArray));
            HttpResponseMessage response = await client.SendAsync(req);
            if (response.IsSuccessStatusCode)
            {
                JObject data = JObject.Parse(await response.Content.ReadAsStringAsync());
                string statusURL = $"{info.BaseURL}/_apis/git/repositories/{info.RepoID}/pullRequests/{info.PullRequestID}/statuses/statuses?api-version=5.1-preview.1";
                HttpRequestMessage statusChange = new HttpRequestMessage(HttpMethod.Post, statusURL);
                statusChange.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(Constants.AzureDevOpsAuthenticationHeaderInstruction, Convert.ToBase64String(byteArray));
                StaleBranchResponse obj;
                if (Int32.Parse(data["behindCount"].ToString()) > 0)
                {
                    obj = CreateStaleBranchResponse("failed", "Behind Branch");
                }
                else
                {
                    obj = CreateStaleBranchResponse("succeeded", "Up to date");

                }
                string json = JsonConvert.SerializeObject(obj);
                statusChange.Content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage statusResponse = await client.SendAsync(statusChange);
                if (!statusResponse.IsSuccessStatusCode)
                {
                    log.LogError("Error occurred when updating CheckStaleBranch status!");
                }
            }
            else
            {
                log.LogError("Error occured when fetching CheckStaleBranch repo info");
            }
        }

        private static StaleBranchResponse CreateStaleBranchResponse(string state, string description)
        {
            return new StaleBranchResponse()
            {
                State = state,
                Description = description,
                Context = new Context()
                {
                    Name = "staleBranch"
                }
            };
        }
    }
}
