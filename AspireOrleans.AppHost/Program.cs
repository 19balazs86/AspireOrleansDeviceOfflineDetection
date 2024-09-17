using Shared;

namespace AspireOrleans.AppHost;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        //--> Define: Resources
        var azureStorage = builder.AddAzureStorage("storage")
            .RunAsEmulator(emulator =>
            {
                emulator.WithBlobPort(10_000)
                        .WithQueuePort(10_001)
                        .WithTablePort(10_002)
                        .WithImageTag("latest");
            });

        var storageTable = azureStorage.AddTables(Constants.AzureTableStorageConnStringName);

        var orleans = builder.AddOrleans("default")
                             .WithClusterId("Devices-Cluster")
                             .WithServiceId("Devices-Service")
                             .WithClustering(storageTable)
                             .WithGrainStorage("Default", storageTable)
                             .WithGrainDirectory(Constants.GrainDirectoryName, storageTable)
                             .WithReminders(storageTable);

        //--> Define: Projects
        var server = builder.AddProject<Projects.OrleansServer>("server")
                            .WithReference(storageTable)
                            .WithReference(orleans)
                            .WithReplicas(2)
                            .WaitFor(azureStorage);

        var client = builder.AddProject<Projects.OrleansClient>("client")
                            .WithReference(orleans.AsClient())
                            .WithReplicas(2)
                            .WaitFor(server);

        builder.Build().Run();
    }
}