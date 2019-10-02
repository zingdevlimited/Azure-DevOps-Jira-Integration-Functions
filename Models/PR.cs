using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace JiraDevOpsIntegrationFunctions.Models
{
    public class PR
    {
        public string ID { get; set; }
        public string name { get; set; }
        public string repoTitle { get; set; }
    }
}
