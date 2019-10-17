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

        [FunctionName(nameof(URLValidation))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", "get", Route = null)] HttpRequest Req,
            [Table(Constants.PullRequestTable)] CloudTable PullRequestsTable)
        {
            dynamic Data = JObject.Parse(await new StreamReader(Req.Body).ReadToEndAsync());
            string Prefix = Data.prefix;
            string PrId = Data.prid;
            string Token = Data.token;
            string HashToken = Utilities.GetHashedToken(Token);
            string RealHashToken = "";
            TableQuery<PRDetail> GetToken = new TableQuery<PRDetail>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Prefix),
                TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, PrId))
                );

            RealHashToken = (await PullRequestsTable.ExecuteQuerySegmentedAsync(GetToken, null)).Results[0].ToString();

            if (RealHashToken == HashToken)
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
