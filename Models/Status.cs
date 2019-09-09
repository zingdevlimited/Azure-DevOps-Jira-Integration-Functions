using System;
using System.Collections.Generic;
using System.Text;

namespace JiraDevOpsIntegrationFunctions.Models
{
    public class Status
    {
        public string op { get; set; }
        public string path { get; set; }
        public string from { get; set; }
        public string value { get; set; }
        public Status()
        {

        }
    }
}
