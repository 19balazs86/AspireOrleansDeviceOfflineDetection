using Shared;

namespace AspireOrleans.AppHost;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        //--> Define: Resources
        var azureStorage = builder.AddAzureStorage("AzureStorage")
            .RunAsEmulator(emulator =>
            {
                emulator.WithBlobPort(10_000)
                        .WithQueuePort(10_001)
                        .WithTablePort(10_002)
                        .WithImageTag("latest");
            });

        var storageTable = azureStorage.AddTables(Constants.AzureTableStorageConnStringName);

        var orleans = builder.AddOrleans("SeviceOrleans")
                             .WithClusterId("DeviceCluster")
                             .WithServiceId("DeviceService")
                             .WithClustering(storageTable)
                             .WithGrainStorage("Default", storageTable)
                             .WithGrainDirectory(Constants.GrainDirectoryName, storageTable)
                             .WithReminders(storageTable);

        //--> Define: Projects
        var server = builder.AddProject<Projects.OrleansServer>("Server")
                            .WithReference(storageTable)
                            .WithReference(orleans)
                            .WithReplicas(2)
                            .WaitFor(azureStorage);

        var client = builder.AddProject<Projects.OrleansClient>("Client")
                            .WithReference(orleans.AsClient())
                            .WithReplicas(2)
                            .WaitFor(server);

        builder.Build().Run();
    }
}