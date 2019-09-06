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
        public string HashedAccessToken { get; set; }
        public string JiraReleasedId { get; set; }

        public PRDetail()
        {
        }
    }
}
