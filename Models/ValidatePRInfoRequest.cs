using System;
using System.Collections.Generic;
using System.Text;

namespace JiraDevOpsIntegrationFunctions.Models
{
    public class ValidatePRInfoRequest
    {
        public string groupId { get; set; }
        public string pullRequestId { get; set; }
        public string token { get; set; }
    }
}
