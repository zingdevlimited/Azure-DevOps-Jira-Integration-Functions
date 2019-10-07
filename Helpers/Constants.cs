namespace JiraDevOpsIntegrationFunctions.Helpers
{
    public class Constants
    {
        public const string ServiceBusConnectionName = "AzureWebJobsServiceBus";
        public const string StaleBranchTriggerName = "CheckStaleBranch";
        public const string JiraIssuesTriggerName = "CheckJiraIssues";
        public const string AzureDevOpsConnectionName = "AzureDevOps_Setting";
        public const string BaseUrlConnectionName = "baseURL";
        public const string SpaUrlConnectionName = "SPAUrl";

        public const string AzureDevOpsAuthenticationHeaderInstruction = "Basic";
        public const string ServiceBus = "prupdated";
        public const string PullRequestTable = "PRDetail";
        public const string IssueMappingTable = "PRIssueMapping";
        public const string PrefixTable = "GroupPrefix";
        public const string RepoMappingTable = "IssueRepoMapping";
    }
}
