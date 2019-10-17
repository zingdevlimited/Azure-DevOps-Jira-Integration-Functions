using JiraDevOpsIntegrationFunctions.Helpers;
using JiraDevOpsIntegrationFunctions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

namespace JiraDevOpsIntegrationFunctions.InternalFunctions
{
    public static class AddOrReplacePRDetailEntry
    {
        [FunctionName(nameof(AddOrReplacePRDetailEntry))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] AddOrReplacePRDetailRequest req,
            [Table(Constants.PullRequestTable)] CloudTable PullRequestsTable)
        {
            if (req == null)
                return new BadRequestResult();

            var Token = Utilities.GetToken(64);
            var AddOrReplaceToken = TableOperation.InsertOrReplace(new PRDetail()
            {
                PartitionKey = req.GroupId,
                RowKey = req.PullRequestId,
                HashedToken = Utilities.GetHashedToken(Token)
            });
            await PullRequestsTable.ExecuteAsync(AddOrReplaceToken);
            return new OkObjectResult(new AddOrReplacePRDetailResponse() { Token = Token });
        }
    }
}
