using JiraDevOpsIntegrationFunctions.Helpers;
using JiraDevOpsIntegrationFunctions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
namespace JiraDevOpsIntegrationFunctions.InternalFunctions
{
    public static class ValidatePRInfo
    {
        [FunctionName(nameof(ValidatePRInfo))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] ValidatePRInfoRequest req,
            [Table(Constants.PullRequestTable, "{groupId}", "{pullRequestId}")] PRDetail match,
            ILogger log)
        {
            string clientId = Environment.GetEnvironmentVariable("JiraClientId", EnvironmentVariableTarget.Process);
            if (string.IsNullOrWhiteSpace(clientId))
                throw new Exception("JiraClientId missing from configuration");

            string clientSecret = Environment.GetEnvironmentVariable("JiraClientSecret", EnvironmentVariableTarget.Process);
            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new Exception("JiraClientSecret missing from configuration");

            if (match == null)
                return new NotFoundResult();

            if (string.IsNullOrWhiteSpace(req.Token) || Utilities.GetHashedToken(req.Token) != match.HashedToken)
                return new UnauthorizedResult();

            var res = new ValidatePRInfoResponse
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Url = Environment.GetEnvironmentVariable(Constants.BaseUrlConnectionName, EnvironmentVariableTarget.Process)
                
            };
            return new OkObjectResult(res);
        }
    }
}
