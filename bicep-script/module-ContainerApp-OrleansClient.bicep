param containerAppEnvId string
param appInsights_ConnString string
param storageAccount_ConnString string

var rgLocation = resourceGroup().location

// --> Container App: OrleansClient
// https://learn.microsoft.com/en-us/azure/templates/microsoft.app/containerapps

resource orleansClientContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'orleans-client'
  location: rgLocation
  properties: {
    managedEnvironmentId: containerAppEnvId
    configuration: {
      maxInactiveRevisions: 5
      ingress: {
        external: true
        targetPort: 8080
      }
    }
    template: {
      containers: [
        {
          name: 'orleans-client'
          image: '19balazs86/orleans-device:client'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [ // Most of the configuration is defined in the appsettings.Production.json
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsights_ConnString
            }
            {
              name: 'ConnectionStrings__AzureTableStorageEndpoint'
              value: storageAccount_ConnString
            }
          ]
        }
      ]
      scale: {
        minReplicas: 2
        maxReplicas: 3
      }
    }
  }
}

output ingressFQDN string = orleansClientContainerApp.properties.configuration.ingress.fqdn