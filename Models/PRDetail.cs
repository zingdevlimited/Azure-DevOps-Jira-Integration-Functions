using Microsoft.WindowsAzure.Storage.Table;

namespace JiraDevOpsIntegrationFunctions.Models
{
    /// <summary>
    /// This class is used to represent PRDetail Entity
    /// </summary>
    public class PRDetail : TableEntity
    {
        public PRDetail() { }
        public string HashedToken { get; set; }
        public string GroupId { get { return this.PartitionKey; } }
        public string PullRequestId { get { return this.RowKey; } }
        public string JiraReleasedId { get; set; }
    }
}
