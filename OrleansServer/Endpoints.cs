using System.Net.Mime;

namespace OrleansServer;

public static class Endpoints
{
    public static void MapDeviceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/dashboard", handleGetDashboard);
    }

    private static async Task handleGetDashboard(HttpContext httpContext)
    {
        using Stream stream = File.OpenRead("dashboard.html");

        httpContext.Response.ContentType = MediaTypeNames.Text.Html;

        await stream.CopyToAsync(httpContext.Response.Body);
    }
}
