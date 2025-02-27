using Orleans.Configuration;
using OrleansServer.FeatureDevice;
using ServiceDefaults;
using StackExchange.Redis;
using System.Net.Mime;

namespace OrleansServer;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        {
            builder.AddServiceDefaults();

            builder.AddKeyedAzureTableClient(Constants.AzureTableStorage_ConnString_Name);

            builder.useOrleans_With_Dashboard();

            builder.addSignalR_With_Backplane();
        }

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline
        {
            app.MapGet("/", () => TypedResults.Content("Hello from OrleansServer | <a href='/dashboard'>Dashboard</a>", MediaTypeNames.Text.Html));

            app.MapDefaultEndpoints();

            app.MapHub<DeviceHub>(DeviceHub.Path);

            app.MapDeviceEndpoints();
        }

        app.Run();
    }

    private static void useOrleans_With_Dashboard(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        int port = builder.Configuration.GetValue<int?>("Orleans:DashboardPort")
            ?? throw new NullReferenceException("Missing configuration: Orleans:DashboardPort");

        builder.UseOrleans(siloBuilder =>
        {
            siloBuilder.UseDashboard(options => options.Port = port);
        });

        services.Configure<AzureTableStorageOptions>(options =>
        {
            options.DeleteStateOnClear = true; // Even if this is set to true, the record does not get deleted
        });

        services.Configure<GrainCollectionOptions>(options =>
        {
            options.CollectionAge = TimeSpan.FromMinutes(5);

            // You can set the "idle timeout" for a specific type with an attribute on the grain class
            // [CollectionAgeLimit(Minutes = 5)]
            // options.ClassSpecificCollectionAge[RuntimeTypeNameFormatter.Format(typeof(DeviceGrain))] = TimeSpan.FromMinutes(5);
        });
    }

    private static void addSignalR_With_Backplane(this IHostApplicationBuilder builder)
    {
        IConfiguration configuration = builder.Configuration;

        bool isDevelopment = builder.Environment.IsDevelopment();

        string? backplaneConnString = isDevelopment ?
            configuration.GetConnectionString(Constants.Redis_ConnString_Name) :
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
