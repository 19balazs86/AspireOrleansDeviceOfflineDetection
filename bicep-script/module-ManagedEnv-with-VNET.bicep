param appName string
param appInsights_ConnectionString string
param logAnalytics_CustomerId string
param logAnalytics_SharedKey string

var rgLocation = resourceGroup().location

// --> Virtual network
// https://learn.microsoft.com/en-us/azure/templates/microsoft.network/virtualnetworks
// https://learn.microsoft.com/en-us/azure/templates/microsoft.network/virtualnetworks/subnets

// https://learn.microsoft.com/en-us/azure/container-apps/vnet-custom

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2023-11-01' = {
  name: '${appName}-VNET'
  location: rgLocation
  properties: {
    addressSpace: {
      addressPrefixes: ['10.0.0.0/16']
    }
    subnets: [
      {
        name: 'infra-subnet'
        properties: {
          addressPrefix: '10.0.0.0/21'
        }
      }
    ]
  }
}

// --> Conainer App - Managed Environments
// https://learn.microsoft.com/en-us/azure/templates/microsoft.app/managedenvironments

resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${appName}-CAE'
  location: rgLocation
  properties: {
    daprAIConnectionString: appInsights_ConnectionString
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics_CustomerId
        sharedKey: logAnalytics_SharedKey
      }
    }
    vnetConfiguration: {
      infrastructureSubnetId: virtualNetwork.properties.subnets[0].id
    }
  }
}

output containerAppEnvId string = containerAppEnv.id