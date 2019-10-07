namespace JiraDevOpsIntegrationFunctions.Models
{
    class PRAndRepoResponse
    {
        public PRAndRepoResponse() { }
        public PR[] PullRequests { get; set; }
        public RepoInfo[] Repos { get; set; }
    }
}
