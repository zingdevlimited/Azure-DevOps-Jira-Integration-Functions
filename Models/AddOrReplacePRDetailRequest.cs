namespace JiraDevOpsIntegrationFunctions.Models
{
    public class AddOrReplacePRDetailRequest
    {
        public AddOrReplacePRDetailRequest() { }
        public string GroupId { get; set; }
        public string PullRequestId { get; set; }
    }
}
