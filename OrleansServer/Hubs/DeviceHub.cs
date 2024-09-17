using Microsoft.AspNetCore.SignalR;

namespace OrleansServer.Hubs;

public interface IDeviceHubClient
{
    Task NotifyStatusChanged(string deviceId, string status);
}

public sealed class DeviceHub : Hub<IDeviceHubClient>
{
    public const string Path = "/hub/devices";

    // No methods for clients to invoke
}
