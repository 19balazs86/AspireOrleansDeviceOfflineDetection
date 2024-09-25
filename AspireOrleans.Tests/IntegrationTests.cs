namespace AspireOrleans.Tests;

public sealed class IntegrationTests : IClassFixture<ApplicationFixture>
{
    private readonly ApplicationFixture _fixture;

    public IntegrationTests(ApplicationFixture applicationFixture)
    {
        _fixture = applicationFixture;
    }

    [Fact]
    public async Task Should_OrleansClient_respond_OK_When_HttpCall_occurs()
    {
        // Act
        using HttpResponseMessage httpResponse = await _fixture.OrleansClient_HttpClient.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task Should_OrleansServer_respond_OK_When_HttpCall_occurs()
    {
        // Act
        using HttpResponseMessage httpResponse = await _fixture.OrleansServer_HttpClient.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
    }

    [Fact]
    public async Task Should_receive_SignalR_Notification_When_Device_receives_Heartbeat()
    {
        // Arrange
        const string expectedDeviceId = "Device01";

        using var manualResetEvent = new ManualResetEventSlim();

        await using HubConnection hubConnection = await _fixture.GetHubConnectionAsync();

        hubConnection.On<string, string>("NotifyStatusChanged", (deviceId, status) =>
        {
            Assert.Equal(expectedDeviceId, deviceId);

            manualResetEvent.Set();
        });

        // Act
        await hubConnection.InvokeAsync<string>("SendHeartbeat", expectedDeviceId);

        manualResetEvent.Wait(TimeSpan.FromSeconds(5));

        // Assert
        Assert.True(manualResetEvent.IsSet);
    }
}
