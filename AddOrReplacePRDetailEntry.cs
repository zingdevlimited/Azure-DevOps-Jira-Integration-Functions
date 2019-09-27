using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using JiraDevOpsIntegrationFunctions.Models;

namespace JiraDevOpsIntegrationFunctions
{
    public static class AddOrReplacePRDetailEntry
    {
        [FunctionName(nameof(AddOrReplacePRDetailEntry))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] AddOrReplacePRDetailRequest req,
            [Table("PRDetail")] CloudTable table,
            ILogger log)
        {
            log.LogInformation("AddOrReplacePRDetailEntry HTTP trigger function processed a request.");
            if (req == null)
                return new BadRequestResult();

            var token = Utilities.GetToken(64);
            var op = TableOperation.InsertOrReplace(new PRDetail()
            {
                PartitionKey = req.groupId,
                RowKey = req.pullRequestId,
                HashedToken = Utilities.GetHashedToken(token)
            });
            await table.ExecuteAsync(op);
            return new OkObjectResult(new AddOrReplacePRDetailResponse() { token = token });
        }
    }
}
