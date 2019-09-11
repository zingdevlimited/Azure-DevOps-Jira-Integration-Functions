using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace JiraDevOpsIntegrationFunctions.Models
{
    public class PRIssueMapping : TableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public PRIssueMapping()
        {
        }
    }
}
