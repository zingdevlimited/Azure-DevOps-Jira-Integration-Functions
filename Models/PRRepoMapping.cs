using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace JiraDevOpsIntegrationFunctions.Models
{
    class PRRepoMapping : TableEntity
    {
        public string MergeStatus { get; set; }
    }
}
