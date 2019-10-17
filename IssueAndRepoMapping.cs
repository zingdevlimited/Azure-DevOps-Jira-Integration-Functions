using JiraDevOpsIntegrationFunctions.Helpers;
using JiraDevOpsIntegrationFunctions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace JiraDevOpsIntegrationFunctions
{
    public static class IssueAndRepoMapping
    {
        [FunctionName(nameof(IssueAndRepoMapping))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
            [Table(Constants.PullRequestTable)] CloudTable DetailTable, 
            [Table(Constants.IssueMappingTable)] CloudTable IssueTable,
            [Table(Constants.RepoMappingTable)] CloudTable RepoTable)
        {            
            if (request == null)
            {
                return new BadRequestResult();
            }

            dynamic Data = JObject.Parse(await new StreamReader(request.Body).ReadToEndAsync());
            string Prefix = Data.Prefix;
            string RequestId = Data.RequestID;
            string Token = Data.token;
            string HashedToken = Utilities.GetHashedToken(Token);
            List<string> IssueIds = new List<string>();
            List<string> Versions = new List<string>();
            foreach(Issue issue in Data.Issues.ToObject<Issue[]>())
            {
                IssueIds.Add(issue.name);
                Versions.Add(issue.version);
            }
            
            RepoMapping[] Repos = Data.RepoMapping.ToObject<RepoMapping[]>();
            TableQuery<PRDetail> GetPr = new TableQuery<PRDetail>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Prefix),
                TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, RequestId))
            );

            if((await DetailTable.ExecuteQuerySegmentedAsync(GetPr, null)).Results.Count != 1)
            {
                return new NotFoundResult();
            }

            foreach (string issueID in IssueIds)
            {
                PRIssueMapping IssueMapping = new PRIssueMapping()
                {
                    PartitionKey = $"{Prefix}|{RequestId}",
                    RowKey = issueID
                };
                TableOperation AddOrReplaceIssueMapping = TableOperation.InsertOrReplace(IssueMapping);
                await IssueTable.ExecuteAsync(AddOrReplaceIssueMapping);
            }

            foreach(RepoMapping repo in Repos)
            {
                foreach(string repoID in repo.Repos)
                {
                    PRRepoMapping repoMapping = new PRRepoMapping()
                    {
                        PartitionKey = $"{Prefix}|{repo.Issue}",
                        RowKey = repoID,
                        MergeStatus = "TODO"
                    };
                    TableOperation AddOrReplaceRepoMapping = TableOperation.InsertOrReplace(repoMapping);
                    await RepoTable.ExecuteAsync(AddOrReplaceRepoMapping);
                }
            }
            await DetailTable.ExecuteAsync(TableOperation.InsertOrMerge(new PRDetail() { PartitionKey = Prefix, RowKey = RequestId, JiraReleasedId = Versions[0] }));
            return new OkResult();
        }
    }
}
