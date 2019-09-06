using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace JiraDevOpsIntegrationFunctions.Models
{
    /// <summary>
    /// This class is used to reperesent GroupPrefix Entity
    /// </summary>
    public class GroupPrefix : TableEntity
    {
        public string Prefix { get; set; }
        public GroupPrefix()
        {
        }
    }
}
