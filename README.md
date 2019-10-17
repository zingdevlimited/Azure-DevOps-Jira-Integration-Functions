# Azure-DevOps-Jira-Integration-Functions
## Overview
This Azure Functions project acts as a backend for Azure DevOps Jira Integration

## Setup
In order to run the backend server you need the following information
- AzureWebJobsStorage - Microsoft Azure Storage Account connection string
- AzureWebJobsServiceBus - Microsoft Azure Service Bus connection string
- AzureDevOps_Setting - Azure DevOps PAT Token
- JiraClientId - Atlassian Jira Client ID (The information becomes available in Attlassian developer portal)
- spaURL - SPA URL which allows user to select the issues and repos linked to that particular Pull Request
- JiraClientSecret - Atlassian Jira Client Secret (The information becomes available in Attlassian developer portal)
- JwtclientKey - The Jira Panel Client Key

The JSON object of your configuration should look similar to the following

```json
{
  "IsEncrypted": false,
  "Host": {
    "CORS": "*"
  },
  "Values": {
    "AzureWebJobsStorage": "<YOUR STORAGE ACCOUNT CONNECTION STRING>",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "AzureWebJobsServiceBus": "<YOUR SERVICE BUS CONNECTION STRING>",
    "AzureDevOps_Setting": "<YOUR AZURE DEVOPS PAT TOKEN>",
    "JiraClientId": "<YOUR JIRA CLIENT ID>",
    "spaURL": "<YOUR SPA URL>",
    "JiraClientSecret": "<YOUR JIRA CLIENT ID>",
    "JwtclientKey": "<YOUR JIRA PANEL CLIENT KEY>"
  }
}
```

After all the configurations are in place the functions project should link up with the other two SPAs.
