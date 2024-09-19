using Shared;
using System.Net.Mime;

namespace OrleansClient;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        {
            builder.AddServiceDefaults();

            builder.AddKeyedAzureTableClient(Constants.AzureTableStorage_ConnString_Name);

            builder.UseOrleansClient();
        }

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline
        {
            app.MapGet("/", () => TypedResults.Content("Hello from OrleansClient | <a href='/send-heartbeat'>Send Heartbeat</a>", MediaTypeNames.Text.Html));

            app.MapDefaultEndpoints();

            app.MapDeviceEndpoints();
        }

        app.Run();
    }
}
