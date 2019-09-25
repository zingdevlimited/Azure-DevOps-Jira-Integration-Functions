using System;
using System.Collections.Generic;
using System.Text;

namespace JiraDevOpsIntegrationFunctions.Models
{
    public class AddOrReplacePRDetailRequest
    {
        public string groupId { get; set; }
        public string pullRequestId { get; set; }
    }
}
