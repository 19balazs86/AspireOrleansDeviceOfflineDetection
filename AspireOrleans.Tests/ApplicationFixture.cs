namespace AspireOrleans.Tests;

public sealed class ApplicationFixture : IAsyncLifetime
{
    private DistributedApplication? _application;

    public HttpClient OrleansClient_HttpClient { get; private set; } = default!;

    public HttpClient OrleansServer_HttpClient { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        const string clientName = "Client";

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireOrleans_AppHost>();

        _application = await appHost.BuildAsync();

        await _application.StartAsync();

        var resourceNotificationService = _application.Services.GetRequiredService<ResourceNotificationService>();

        await resourceNotificationService.WaitForResourceAsync(clientName, KnownResourceStates.Running)
                                         .WaitAsync(TimeSpan.FromSeconds(30));

        OrleansClient_HttpClient = _application.CreateHttpClient(clientName);
        OrleansServer_HttpClient = _application.CreateHttpClient("Server");
    }

    public async Task DisposeAsync()
    {
        if (_application is not null)
        {
            await _application.StopAsync();

            await _application.DisposeAsync();
        }
    }
}
