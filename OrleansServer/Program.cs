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

            builder.UseOrleans();

            builder.addSignalR_WithBackplane();
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

    private static void addSignalR_WithBackplane(this IHostApplicationBuilder builder)
    {
        IServiceCollection services  = builder.Services;
        IConfiguration configuration = builder.Configuration;
        bool isDevelopment           = builder.Environment.IsDevelopment();

        string? backplaneConnString = isDevelopment ?
            configuration.GetConnectionString("Redis") :
            configuration.GetConnectionString("AzureSignalR");

        ArgumentException.ThrowIfNullOrWhiteSpace(backplaneConnString);

        var signalR = services.AddSignalR();

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
