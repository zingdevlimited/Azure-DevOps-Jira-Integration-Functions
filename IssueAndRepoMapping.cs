using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Table;
using JiraDevOpsIntegrationFunctions.Models;
using Newtonsoft.Json.Linq;

namespace JiraDevOpsIntegrationFunctions
{
    public static class IssueAndRepoMapping
    {
        [FunctionName("IssueAndRepoMapping")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
            [Table("PRDetail")] CloudTable detailTable, [Table("PRIssueMapping")] CloudTable issueTable,
            [Table("IssueRepoMapping")] CloudTable repoTable)
        {            
            if (request == null)
            {
                return new BadRequestResult();
            }

            dynamic data = JObject.Parse(await new StreamReader(request.Body).ReadToEndAsync());
            string prefix = data.Prefix;
            string requestID = data.RequestID;
            string issueID = data.IssueID;
            string repoID = data.RepoID;
            int records = 0;

            TableQuery<PRDetail> rangeQuery = new TableQuery<PRDetail>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, prefix),
                TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, requestID))
                );

            foreach (PRDetail item in await detailTable.ExecuteQuerySegmentedAsync(rangeQuery, null))
            {
                records++;
            }

            if(records != 1)
            {
                return new NotFoundResult();
            }

            PRIssueMapping issueMapping = new PRIssueMapping()
            {
                PartitionKey = $"{prefix}|{requestID}",
                RowKey = issueID
            };

            PRRepoMapping repoMapping = new PRRepoMapping()
            {
                PartitionKey = $"{prefix}|{issueID}",
                RowKey = repoID, MergeStatus = "TODO"
            };

            TableOperation operation = TableOperation.InsertOrReplace(issueMapping);
            TableOperation operation2 = TableOperation.InsertOrReplace(repoMapping);

            await issueTable.ExecuteAsync(operation);
            await repoTable.ExecuteAsync(operation2);
            return new OkResult();
        }
    }
}
