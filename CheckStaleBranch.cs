using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JiraDevOpsIntegrationFunctions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
                dynamic data = JsonConvert.SerializeObject((response.Content).ReadAsStringAsync());
                if(data.behindCount > 0)
                {
                    string statusURL = $"{info.BaseURL}_apis/git/repositories/{info.RepoID}/pullRequests/{info.PullRequestID}/statuses?api-version=5.1-preview.1";
                    HttpRequestMessage statusChange = new HttpRequestMessage(HttpMethod.Patch, statusURL);
                    statusChange.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    //statusChange.Content = new StringContent(status, Encoding.UTF8, "application/json");
                }
                else
                {

                }
            }
            else
            {
                log.LogError("Error");
            }
        }
    }
}
