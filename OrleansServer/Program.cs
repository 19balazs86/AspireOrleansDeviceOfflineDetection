using OrleansServer.Hubs;
using Shared;
using StackExchange.Redis;

namespace OrleansServer;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        IServiceCollection services   = builder.Services;

        // Add services to the container
        {
            builder.AddServiceDefaults();

            builder.AddKeyedAzureTableClient(Constants.AzureTableStorageConnStringName);

            builder.useOrleans_With_Dashboard();

            builder.addSignalR_With_Backplane();
        }

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline
        {
            app.MapGet("/", () => "Hello from OrleansServer");

            app.MapDefaultEndpoints();

            app.MapHub<DeviceHub>(DeviceHub.Path);

            app.MapDeviceEndpoints();
        }

        app.Run();
    }

    private static void useOrleans_With_Dashboard(this IHostApplicationBuilder builder)
    {
        int port = builder.Configuration.GetValue<int?>("OrleansDashboardPort")
            ?? throw new NullReferenceException("Missing configuration: OrleansDashboardPort");

        builder.UseOrleans(siloBuilder =>
        {
            siloBuilder.UseDashboard(options => options.Port = port);
        });
    }

    private static void addSignalR_With_Backplane(this IHostApplicationBuilder builder)
    {
        IConfiguration configuration = builder.Configuration;

        bool isDevelopment = builder.Environment.IsDevelopment();

        string? backplaneConnString = isDevelopment ?
            configuration.GetConnectionString("Redis") :
            configuration.GetConnectionString("AzureSignalR");

        ArgumentException.ThrowIfNullOrWhiteSpace(backplaneConnString);

        var signalR = builder.Services.AddSignalR();

        if (isDevelopment)
        {
            signalR.AddStackExchangeRedis(backplaneConnString, options =>
                options.Configuration.ChannelPrefix = RedisChannel.Literal("DeviceBackplane"));
        }
        else
        {
            signalR.AddAzureSignalR(backplaneConnString);
        }
    }
}
