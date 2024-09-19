param containerAppEnvId string
param appInsights_ConnString string
param storageAccount_ConnString string
param signalR_ConnString string

var rgLocation = resourceGroup().location

// --> Container App: OrleansServer
// https://learn.microsoft.com/en-us/azure/templates/microsoft.app/containerapps

resource orleansServerContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: 'orleans-server'
  location: rgLocation
  properties: {
    managedEnvironmentId: containerAppEnvId
    configuration: {
      maxInactiveRevisions: 5
      ingress: {
        additionalPortMappings: [
          {
            external: true
            targetPort: 8585
          }
          {
            external: false
            targetPort: 8000
          }
          {
            external: false
            targetPort: 8001
          }
        ]
        external: true
        targetPort: 8080
      }
    }
    template: {
      containers: [
        {
          name: 'orleans-server'
          image: '19balazs86/orleans-device:server'
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
            {
              name: 'ConnectionStrings__AzureSignalR'
              value: signalR_ConnString
            }
          ]
        }
      ]
      scale: {
        minReplicas: 2
        maxReplicas: 3
        rules: [
          {
            name: 'rule-memory'
            custom: {
              type: 'memory'
              metadata: {
                type: 'Utilization'
                value: '85'
              }
            }
          }
        ]
      }
    }
  }
}

output ingressFQDN string = orleansServerContainerApp.properties.configuration.ingress.fqdn