namespace JiraDevOpsIntegrationFunctions.Models
{
    public class RepoMapping
    {
        public RepoMapping() { }
        public string Issue { get; set; }
        public string[] Repos { get; set; }
    }
}
