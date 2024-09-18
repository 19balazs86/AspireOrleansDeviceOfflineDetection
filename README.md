# Playing with device offline detection using Orleans

- The idea for this concept came from [Device Offline detection with Durable Entities](https://dev.to/azure/device-offline-detection-with-durable-entities-e8g) by Kees Schollaart
- I wanted to experiment with it and learn from the process
- I have another solution using Azure Functions with Durable Entities, which can be [found here](https://github.com/19balazs86/PlayingWithDeviceOfflineDetection)

## In this repository/solution

I have adopted the version of Azure Functions with Durable Entities in the style of the actor model using Orleans. The concept is simple, but incorporates the following features working together:

- **SignalR** with Backplane, using Redis for local development and Azure SignalR in Azure cloud
- **Orleans**:
  - Both server (Silo cluster) and client have multiple replicas
  - Azure Table Storage for clustering, grain storage, grain directory, and reminders
  - Dashboard
- Distributed tracing with **OpenTelemetry**
- **.NET Aspire** bringing everything together, making it a pleasure to work with

## Running it locally

- Docker Desktop is required to run the application
- URLs:
  - Device Dashboard (with SignalR): https://localhost:7211/dashboard
  - Send heartbeat (random devices) with the client: https://localhost:7251/send-heartbeat
  - Orleans Dashboard: [http://localhost:8585](http://localhost:8585)


## Resources

#### .NET Aspire

- [Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview) 📚*MS-Learn*
  - [Integrations](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/integrations-overview)
  - [Orleans with .NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/frameworks/orleans)

#### Orleans

- [Documentation](https://learn.microsoft.com/en-us/dotnet/orleans/overview) 📚*MS-Learn*
  - [Best practices](https://learn.microsoft.com/en-us/dotnet/orleans/resources/best-practices)
  - [NuGet packages](https://learn.microsoft.com/en-us/dotnet/orleans/resources/nuget-packages)
- [Playing with Microsoft Orleans](https://github.com/19balazs86/PlayingWithOrleans) 👤
