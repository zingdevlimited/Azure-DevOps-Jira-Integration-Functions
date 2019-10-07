using JiraDevOpsIntegrationFunctions.Helpers;
using JiraDevOpsIntegrationFunctions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace JiraDevOpsIntegrationFunctions
{
    public static class IssueAndRepoMapping
    {
        [FunctionName("IssueAndRepoMapping")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
            [Table(Constants.PullRequestTable)] CloudTable detailTable, 
            [Table(Constants.IssueMappingTable)] CloudTable issueTable,
            [Table(Constants.RepoMappingTable)] CloudTable repoTable)
        {            
            if (request == null)
            {
                return new BadRequestResult();
            }

            dynamic data = JObject.Parse(await new StreamReader(request.Body).ReadToEndAsync());
            string prefix = data.Prefix;
            string[] issueIDs = data.IssueID.ToObject<string[]>();
            string requestID = data.RequestID;
            string token = data.token;
            string hashedToken = Utilities.GetHashedToken(token);
            
            RepoMapping[] repos = data.RepoMapping.ToObject<RepoMapping[]>();
            int records = 0;

            TableQuery<PRDetail> rangeQuery = new TableQuery<PRDetail>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, prefix),
                TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, requestID))
                );

            foreach (PRDetail item in await detailTable.ExecuteQuerySegmentedAsync(rangeQuery, null))
            {
                if(item.HashedToken == hashedToken)
                {
                    records++;
                }
                else
                {
                    return new NotFoundResult();
                }
            }

            if(records != 1)
            {
                return new NotFoundResult();
            }

            foreach (string issueID in issueIDs)
            {
                PRIssueMapping issueMapping = new PRIssueMapping()
                {
                    PartitionKey = $"{prefix}|{requestID}",
                    RowKey = issueID
                };
                TableOperation operation = TableOperation.InsertOrReplace(issueMapping);
                await issueTable.ExecuteAsync(operation);
            }

            foreach(RepoMapping repo in repos)
            {
                foreach(string repoID in repo.Repos)
                {
                    PRRepoMapping repoMapping = new PRRepoMapping()
                    {
                        PartitionKey = $"{prefix}|{repo.Issue}",
                        RowKey = repoID,
                        MergeStatus = "TODO"
                    };
                    TableOperation operation2 = TableOperation.InsertOrReplace(repoMapping);
                    await repoTable.ExecuteAsync(operation2);
                }
            }
            return new OkResult();
        }
    }
}
