using System;
using System.Collections.Generic;
using System.Text;

namespace JiraDevOpsIntegrationFunctions.Models
{
    class PRAndRepoResponse
    {
        public PR[] PullRequests { get; set; }
        public RepoInfo[] Repos { get; set; }
    }
}
