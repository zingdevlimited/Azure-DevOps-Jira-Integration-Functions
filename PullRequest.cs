using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using ProjectFunctions.Models;
using JiraDevOpsIntegrationFunctions;
using JiraDevOpsIntegrationFunctions.Models;

namespace ProjectFunctions
{
    public static class PullRequest
    {
        [FunctionName("PullRequest")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, Binder binder, [Table("PRDetail")] IAsyncCollector<PRDetail> PullRequestDetail,
            [ServiceBus("prupdated", Connection = "AzureWebJobsServiceBus")] IAsyncCollector<PRInfo> topic)
        {
            dynamic data;
            string RequestID;            
            GroupPrefix prefix;
            try
            {
                data = JObject.Parse(await new StreamReader(req.Body).ReadToEndAsync());
                RequestID = data["resource"]["pullRequestId"];
                string RowKey = data.resourceContainers.project.id;
                string PartitionKey = data.resourceContainers.project.baseUrl;
                string source = data.resource.sourceRefName;
                string target = data.resource.targetRefName;
                string repoID = data.resource.repository.id;
                string PRID = data.resource.pullRequestId;
                source = source.Replace("refs/heads/", "");
                target = target.Replace("refs/heads/", "");
                if (string.IsNullOrEmpty(RequestID))
                {
                    return new NotFoundResult();
                }

                prefix = binder.Bind<GroupPrefix>(new TableAttribute("GroupPrefix", PartitionKey.Replace("https://","").Replace("/"," "), RowKey));

                if (prefix == null)
                {
                    return new NotFoundResult();
                }

                if (data.eventType == "git.pullrequest.created")
                {
                    await PullRequestDetail.AddAsync(new PRDetail { PartitionKey = prefix.Prefix, RowKey = RequestID, HashedAccessToken = GetToken(64), JiraReleasedId = "" });
                }
                await topic.AddAsync(new PRInfo() { Prefix = prefix.Prefix, PRId = RequestID, Source = source, Target = target, BaseURL = PartitionKey, RepoID = repoID, PullRequestID = PRID });
            }
            catch
            {
                return new BadRequestResult();
            }
            return new OkResult();
        }

        /// <summary>
        /// This method is used in Access Token generation
        /// </summary>
        /// <param name="length">The length of the Token</param>
        /// <returns>Access Token</returns>
        public static string GetToken(int length)
        {
            const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            string Token = "";
            RNGCryptoServiceProvider Provider = new RNGCryptoServiceProvider();
            while (Token.Length != length)
            {
                byte[] oneByte = new byte[1];
                Provider.GetBytes(oneByte);
                char character = (char)oneByte[0];
                if (Chars.Contains(character))
                {
                    Token += character;
                }
            }
            return Token;
        }
    }
}
