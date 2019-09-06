using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using ProjectFunctions.Models;

namespace ProjectFunctions
{
    public static class CheckStaleBranch
    {
        [FunctionName("CheckStaleBranch")]
        public static void Run([ServiceBusTrigger("prupdated", "CheckStaleBranch", Connection = "AzureWebJobsServiceBus")]PRInfo info, ILogger log)
        {
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {info.Prefix}");
        }
    }
}
