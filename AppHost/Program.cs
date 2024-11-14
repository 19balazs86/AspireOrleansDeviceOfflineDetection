using Microsoft.Extensions.Hosting;
using ServiceDefaults;

namespace AppHost;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        bool isDevelopment = builder.Environment.IsDevelopment();

        //--> Define: Resources
        IResourceBuilder<RedisResource>? redis = null;

        if (isDevelopment)
        {
            redis = builder.AddRedis(Constants.Redis_ConnString_Name, port: 52369)
                           .WithImageTag("latest");
        }

        var azureStorage = builder.AddAzureStorage("AzureStorage")
            .RunAsEmulator(emulator =>
            {
                emulator.WithBlobPort(10_000)
                        .WithQueuePort(10_001)
                        .WithTablePort(10_002)
                        .WithImageTag("latest");
            });

        var storageTable = azureStorage.AddTables(Constants.AzureTableStorage_ConnString_Name);

        var orleans = builder.AddOrleans("SeviceOrleans")
                             .WithClusterId("DeviceCluster")
                             .WithServiceId("DeviceService")
                             .WithClustering(storageTable)
                             .WithGrainStorage("Default", storageTable)
                             .WithGrainDirectory(Constants.GrainDirectory_Name, storageTable)
                             .WithReminders(storageTable);

        //--> Define: Projects
        var server = builder.AddProject<Projects.OrleansServer>("Server")
                            .WithExternalHttpEndpoints()
                            .WithHttpEndpoint(name: "OrleansDashboard", env: "Orleans__DashboardPort", port: 8585)
                            .WithReference(storageTable)
                            .WithReference(orleans)
                            .WithReplicas(2)
                            .WaitFor(azureStorage);

        if (redis is not null)
        {
            server.WithReference(redis)
                  .WaitFor(redis);
        }

        var client = builder.AddProject<Projects.OrleansClient>("Client")
                            .WithExternalHttpEndpoints()
                            .WithReference(orleans.AsClient())
                            .WithReplicas(2)
                            .WaitFor(server);

        builder.Build().Run();
    }
}
