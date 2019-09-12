using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace JiraDevOpsIntegrationFunctions
{
    public static class URLValidation
    {
        public static object GetHashedToken { get; private set; }

        [FunctionName("URLValidation")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", "get", Route = null)] HttpRequest req,
            [Table("PRDetail")] CloudTable cloudTable,
            ILogger log)
        {
            PRDetail record = new PRDetail();
            dynamic data = JObject.Parse(await new StreamReader(req.Body).ReadToEndAsync());
            string prefix = data.prefix;
            string prid = data.prid;
            string token = data.token;
            string hashToken = HashToken(token);
            string realHashToken = "";
            TableQuery<PRDetail> rangeQuery = new TableQuery<PRDetail>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, prefix),
                TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, prid))
                );
            foreach (PRDetail item in await cloudTable.ExecuteQuerySegmentedAsync(rangeQuery, null))
            {
                realHashToken = item.HashedAccessToken;
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

        public static string HashToken(string token)
        {
            SHA256 sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToUpper();
        }
    }
}
