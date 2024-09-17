using Shared;

namespace OrleansClient;

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

            builder.UseOrleansClient();
        }

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline
        {
            app.MapGet("/", () => "Hello from OrleansClient");

            app.MapDefaultEndpoints();

            app.MapDeviceEndpoints();
        }

        app.Run();
    }
}
