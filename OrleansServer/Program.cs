using OrleansServer.Hubs;
using Shared;

namespace OrleansServer;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        IServiceCollection services   = builder.Services;
        IConfiguration configuration  = builder.Configuration;

        // Add services to the container
        {
            builder.AddServiceDefaults();

            builder.AddKeyedAzureTableClient(Constants.AzureTableStorageConnStringName);

            builder.UseOrleans();

            services.AddSignalR();
        }

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline
        {
            app.MapGet("/", () => "Hello from OrleansServer");

            app.MapDefaultEndpoints();

            app.MapHub<DeviceHub>(DeviceHub.Path);
        }

        app.Run();
    }
}
