using JiraDevOpsIntegrationFunctions.Helpers;
using JiraDevOpsIntegrationFunctions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using ProjectFunctions.Models;
using System.IO;
using System.Threading.Tasks;

namespace ProjectFunctions
{
    public static class PullRequest
    {
        [FunctionName("PullRequest")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, Binder binder, [Table(Constants.PullRequestTable)] IAsyncCollector<PRDetail> PullRequestDetail,
            [ServiceBus(Constants.ServiceBus, Connection = Constants.ServiceBusConnectionName)] IAsyncCollector<PRInfo> topic)
        {
            try
            {
                dynamic Data = JObject.Parse(await new StreamReader(req.Body).ReadToEndAsync());
                string RequestID = Data.resource.pullRequestId;
                string RowKey = Data.resourceContainers.project.id;
                string PartitionKey = Data.resourceContainers.project.baseUrl;
                string Source = Data.resource.sourceRefName;
                string Target = Data.resource.targetRefName;
                string RepoId = Data.resource.repository.id;
                string PrId = Data.resource.pullRequestId;
                Source = Source.Replace("refs/heads/", "");
                Target = Target.Replace("refs/heads/", "");

                if (string.IsNullOrEmpty(RequestID))
                {
                    return new NotFoundResult();
                }

                GroupPrefix prefix = binder.Bind<GroupPrefix>(new TableAttribute("GroupPrefix", PartitionKey.Replace("https://","").Replace("/"," "), RowKey));

                if (prefix == null)
                {
                    return new NotFoundResult();
                }

                string token = Utilities.GetToken(64);
                string hashedToken = Utilities.GetHashedToken(token);
                if (Data.eventType == "git.pullrequest.created")
                {
                    await PullRequestDetail.AddAsync(new PRDetail { PartitionKey = prefix.Prefix, RowKey = RequestID, JiraReleasedId = "", HashedToken = hashedToken });
                }
                await topic.AddAsync(new PRInfo() { Prefix = prefix.Prefix, PRId = RequestID, Source = Source, Target = Target, BaseURL = PartitionKey, RepoID = RepoId, PullRequestID = PrId, Token = token });
            }
            catch
            {
                return new BadRequestResult();
            }
            return new OkResult();
        }
    }
}
