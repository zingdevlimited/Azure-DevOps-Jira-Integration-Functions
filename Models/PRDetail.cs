using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace JiraDevOpsIntegrationFunctions
{
    /// <summary>
    /// This class is used to represent PRDetail Entity
    /// </summary>
    public class PRDetail : TableEntity
    {
        public string HashedToken { get; set; }
        public string GroupId { get { return this.PartitionKey; } }
        public string PullRequestId { get { return this.RowKey; } }
        public string JiraReleasedId { get; set; }
        public PRDetail()
        {
        }
    }
}
