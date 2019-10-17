using JiraDevOpsIntegrationFunctions.Helpers;
using JiraDevOpsIntegrationFunctions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;

namespace JiraDevOpsIntegrationFunctions
{
    public static class ValidatePRInfo
    {
        [FunctionName(nameof(ValidatePRInfo))]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] ValidatePRInfoRequest req,
            [Table(Constants.PullRequestTable, "{groupId}", "{pullRequestId}")] PRDetail Match)
        {
            if (Match == null)
                return new NotFoundResult();

            if (string.IsNullOrWhiteSpace(req.Token) || Utilities.GetHashedToken(req.Token) != Match.HashedToken)
                return new UnauthorizedResult();

            var res = new ValidatePRInfoResponse
            {
                ClientId = Environment.GetEnvironmentVariable("JiraClientId", EnvironmentVariableTarget.Process),
                ClientSecret = Environment.GetEnvironmentVariable("JiraClientSecret", EnvironmentVariableTarget.Process)
            };
            return new OkObjectResult(res);
        }
    }
}
