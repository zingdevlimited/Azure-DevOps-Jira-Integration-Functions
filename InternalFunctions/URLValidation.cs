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

namespace JiraDevOpsIntegrationFunctions.InternalFunctions
{
    public static class URLValidation
    {
        public static object GetHashedToken { get; private set; }

        [FunctionName("URLValidation")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", "get", Route = null)] HttpRequest req,
            [Table(Constants.PullRequestTable)] CloudTable cloudTable,
            ILogger log)
        {
            dynamic data = JObject.Parse(await new StreamReader(req.Body).ReadToEndAsync());
            string prefix = data.prefix;
            string prid = data.prid;
            string token = data.token;
            string hashToken = Utilities.GetHashedToken(token);
            string realHashToken = "";
            TableQuery<PRDetail> rangeQuery = new TableQuery<PRDetail>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, prefix),
                TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, prid))
                );
            foreach (PRDetail item in await cloudTable.ExecuteQuerySegmentedAsync(rangeQuery, null))
            {
                realHashToken = item.HashedToken;
            }

            if (realHashToken == hashToken)
            {
                return new OkResult();
            }
            else
            {
                return new NotFoundResult();
            }            
        }
    }
}
