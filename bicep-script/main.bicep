param appName string

var rgLocation = resourceGroup().location

var nameToken = substring(uniqueString(resourceGroup().id), 0, 4)

var storageAccountName = toLower('${appName}${nameToken}')

// --> Storage account
// https://learn.microsoft.com/en-us/azure/templates/microsoft.storage/storageaccounts

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: rgLocation
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}

var storageAccountConnString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'

// --> Module: LogAnalytics workspace + Application Insights
// https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/modules

module moduleAppInsights 'module-AppInsightsLogAnalytics.bicep' = {
  name: 'Insights-Deployment'
  params: {
    appName: appName
  }
}

// --> Module: Managed Environment + VNET

module moduleContainerAppEnv 'module-ManagedEnv-with-VNET.bicep' = {
  name: 'ManagedEnv-Deployment'
  params: {
    appName: appName
    appInsights_ConnectionString: moduleAppInsights.outputs.appInsights_ConnectionString
    logAnalytics_CustomerId: moduleAppInsights.outputs.logAnalytics_CustomerId
    logAnalytics_SharedKey: moduleAppInsights.outputs.logAnalytics_SharedKey
  }
}

// --> SignalR
// https://learn.microsoft.com/en-us/azure/templates/microsoft.signalrservice/signalr

resource signalR 'Microsoft.SignalRService/signalR@2023-08-01-preview' = {
  name: '${appName}${nameToken}-SIGR'
  location: rgLocation
  sku: {
    name: 'Free_F1'
  }
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: 'Default'
      }
    ]
  }
}

// --> Module: Container App: OrleansServer

module moduleContainerAppOrleansServer 'module-ContainerApp-OrleansServer.bicep' = {
  name: 'OrleansServer-Deployment'
  params: {
    containerAppEnvId: moduleContainerAppEnv.outputs.containerAppEnvId
    appInsights_ConnString: moduleAppInsights.outputs.appInsights_ConnectionString
    storageAccount_ConnString: storageAccountConnString
    signalR_ConnString: signalR.listKeys().primaryConnectionString
  }
}

// --> Module: Container App: OrleansClient

module moduleContainerAppOrleansClient 'module-ContainerApp-OrleansClient.bicep' = {
  name: 'OrleansClient-Deployment'
  params: {
    containerAppEnvId: moduleContainerAppEnv.outputs.containerAppEnvId
    appInsights_ConnString: moduleAppInsights.outputs.appInsights_ConnectionString
    storageAccount_ConnString: storageAccountConnString
  }
}

output OrleansServerURL string = moduleContainerAppOrleansServer.outputs.ingressFQDN
output OrleansClientURL string = moduleContainerAppOrleansClient.outputs.ingressFQDN