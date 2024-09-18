using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        // builder.Services.AddServiceDiscovery();

        //builder.Services.ConfigureHttpClientDefaults(httpClientBuilder =>
        //{
        //    httpClientBuilder.AddStandardResilienceHandler();

        //    httpClientBuilder.AddServiceDiscovery();
        //});

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddMeter("Microsoft.Orleans");
                       //.AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation();

                // tracing.AddSource("Microsoft.Orleans.Runtime"); // It produces numerous tracing records every second

                tracing.AddSource("Microsoft.Orleans.Application");
            })
            .WithLogging(lpb => { }, logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes           = true;
            });

        builder.addOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder addOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        IConfiguration configuration = builder.Configuration;

        OpenTelemetryBuilder telemetryBuilder = builder.Services.AddOpenTelemetry();

        bool useOtlpExporter = !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            telemetryBuilder.UseOtlpExporter();
        }

        if (builder.Environment.IsProduction())
        {
            bool useAzureMonitor = !string.IsNullOrWhiteSpace(configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);

            if (useAzureMonitor)
            {
                telemetryBuilder.UseAzureMonitor();
            }
        }

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks("/health");

            app.MapHealthChecks("/alive", _liveHealthCheckOptions);
        }

        return app;
    }

    private static readonly HealthCheckOptions _liveHealthCheckOptions = new () { Predicate = r => r.Tags.Contains("live") };
}
