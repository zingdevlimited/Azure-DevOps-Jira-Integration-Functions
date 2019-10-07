using Microsoft.WindowsAzure.Storage.Table;

namespace JiraDevOpsIntegrationFunctions.Models
{
    class PRRepoMapping : TableEntity
    {
        public PRRepoMapping() { }
        public string MergeStatus { get; set; }
    }
}
