namespace JiraDevOpsIntegrationFunctions.Models
{
    public class ValidatePRInfoRequest
    {
        public string GroupId { get; set; }
        public string PullRequestId { get; set; }
        public string Token { get; set; }
    }
}
