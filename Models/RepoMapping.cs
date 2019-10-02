using System;
using System.Collections.Generic;
using System.Text;

namespace JiraDevOpsIntegrationFunctions.Models
{
    public class RepoMapping
    {
        public RepoMapping() { }
        public string issue { get; set; }
        public string[] repos { get; set; }
    }
}
