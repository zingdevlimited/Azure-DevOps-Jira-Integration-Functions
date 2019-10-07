using Microsoft.WindowsAzure.Storage.Table;

namespace JiraDevOpsIntegrationFunctions.Models
{
    /// <summary>
    /// This class is used to reperesent GroupPrefix Entity
    /// </summary>
    public class GroupPrefix : TableEntity
    {
        public GroupPrefix() { }
        public string Prefix { get; set; }
    }
}
