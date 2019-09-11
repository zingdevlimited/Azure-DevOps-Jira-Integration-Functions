using System;
using System.Collections.Generic;
using System.Text;

namespace JiraDevOpsIntegrationFunctions.Models
{
    class JiraIssueModel
    {
        public string state { get; set; }
        public string description { get; set; }
        public Context context { get; set; }
        public string targetUrl { get; set; }
    }
}
