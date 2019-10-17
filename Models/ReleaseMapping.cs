using Microsoft.WindowsAzure.Storage.Table;

namespace JiraDevOpsIntegrationFunctions.Models
{
    public class ReleaseMapping : TableEntity
    {
        public string MergeStatus { get; set; }
    }
}
