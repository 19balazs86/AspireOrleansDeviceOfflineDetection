using Microsoft.AspNetCore.SignalR;
using Orleans.GrainDirectory;
using OrleansServer.GrainStates;
using OrleansServer.Hubs;
using Shared;

namespace OrleansServer.Grains;

[GrainDirectory(Constants.GrainDirectoryName)]
public sealed class DeviceGrain : Grain, IDeviceGrain
{
    private readonly ILogger<DeviceGrain> _logger;

    private readonly IPersistentState<DeviceState> _state;

    private readonly IHubContext<DeviceHub, IDeviceHubClient> _deviceHub;

    private readonly string _deviceId;

    public DeviceGrain(
        ILogger<DeviceGrain> logger,
        [PersistentState(nameof(DeviceState))] IPersistentState<DeviceState> state,
        IHubContext<DeviceHub, IDeviceHubClient> deviceHub)
    {
        _logger    = logger;
        _state     = state;
        _deviceHub = deviceHub;
        _deviceId  = this.GetPrimaryKeyString();
    }

    public async Task ReceiveHeartbeat()
    {
        _logger.LogInformation("Device({name}) received a heartbeat", _deviceId);

        _state.State.LastHeartbeat = DateTime.UtcNow;
        _state.State.HeartbeatCount++;

        await _state.WriteStateAsync();

        await _deviceHub.Clients.All.NotifyStatusChanged(_deviceId, "online"); // offline
    }
}
