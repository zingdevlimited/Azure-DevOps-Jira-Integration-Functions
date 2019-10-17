using JiraDevOpsIntegrationFunctions.Helpers;
using JiraDevOpsIntegrationFunctions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace JiraDevOpsIntegrationFunctions
{
    public static class MergeBranchUpdate
    {
        [FunctionName(nameof(MergeBranchUpdate))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Table(Constants.RepoMappingTable)] CloudTable RepoMappingTable,
            [Table(Constants.PrefixTable)] CloudTable PrefixTable,
            [Table(Constants.IssueMappingTable)] CloudTable IssueMappingTable,
            [Table(Constants.ReleaseTable)] CloudTable ReleaseTable,
            [Table(Constants.PullRequestTable)] CloudTable PullRequestTable,
            ILogger log)
        {
            dynamic Data = JObject.Parse(await new StreamReader(req.Body).ReadToEndAsync());
            string Status = Data.resource.status;
            if (Status == "completed")
            {
                bool MergeStatusCheck = true;
                string ProjectId = Data.resourceContainers.project.id;
                string Url = Data.resourceContainers.project.baseUrl;
                string RepoId = Data.resource.repository.id;
                string PrId = Data.resource.pullRequestId;
                Url = Url.Replace("https://", "").Replace("/", " ");

                TableQuery<GroupPrefix> GetPrefix = new TableQuery<GroupPrefix>().Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Url),
                TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, ProjectId))
                );

                string Prefix = (await PrefixTable.ExecuteQuerySegmentedAsync(GetPrefix, null)).Results[0].Prefix;
                TableQuery<PRIssueMapping> GetJiraIds = new TableQuery<PRIssueMapping>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{Prefix}|{PrId}"));
                foreach(PRIssueMapping issue in await IssueMappingTable.ExecuteQuerySegmentedAsync(GetJiraIds, null))
                {
                    TableQuery<PRRepoMapping> GetCorrectEntries = new TableQuery<PRRepoMapping>().Where(TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{Prefix}|{issue.RowKey}"),
                    TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, RepoId))
                    );
                    foreach (PRRepoMapping RepoEntry in await RepoMappingTable.ExecuteQuerySegmentedAsync(GetCorrectEntries, null))
                    {
                        TableOperation replace = TableOperation.InsertOrReplace(new PRRepoMapping()
                        {
                            PartitionKey = RepoEntry.PartitionKey,
                            RowKey = RepoEntry.RowKey,
                            MergeStatus = "DONE"
                        });
                        await RepoMappingTable.ExecuteAsync(replace);
                    }
                }
                TableQuery<PRRepoMapping> GetRepoInfo = new TableQuery<PRRepoMapping>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, RepoId));
                foreach (PRRepoMapping RepoEntry in await RepoMappingTable.ExecuteQuerySegmentedAsync(GetRepoInfo, null))
                {
                    if (RepoEntry.MergeStatus == "TODO")
                    {
                        MergeStatusCheck = false;
                    }
                }
                if (MergeStatusCheck)
                {
                    TableQuery<PRDetail> GetPrJiraReleaseId = new TableQuery<PRDetail>().Where(TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{Prefix}"),
                    TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, $"{PrId}"))
                    );
                    string ReleaseId = (await PullRequestTable.ExecuteQuerySegmentedAsync(GetPrJiraReleaseId, null)).Results[0].JiraReleasedId;
                    TableOperation Add = TableOperation.Insert(new ReleaseMapping() 
                    {
                        PartitionKey = $"{Prefix}|{ReleaseId}",
                        RowKey = RepoId,
                        MergeStatus = "DONE" 
                    });
                    await ReleaseTable.ExecuteAsync(Add);
                }
            }
            return new OkResult();
        }
    }
}
