using Microsoft.AspNetCore.SignalR;
using Shared;

namespace OrleansServer.FeatureDevice;

public interface IDeviceHubClient
{
    Task NotifyStatusChanged(string deviceId, string status);
}

public interface IDeviceHub
{
    Task SendHeartbeat(string deviceId);
}

public sealed class DeviceHub : Hub<IDeviceHubClient>, IDeviceHub
{
    public const string Path = "/hub/devices";

    private readonly IGrainFactory _grainFactory;

    public DeviceHub(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task SendHeartbeat(string deviceId)
    {
        var deviceGrain = _grainFactory.GetGrain<IDeviceGrain>(deviceId);

        await deviceGrain.ReceiveHeartbeat();
    }
}
