using Orleans.GrainDirectory;
using OrleansServer.GrainStates;
using Shared;

namespace OrleansServer.Grains;

[GrainDirectory(Constants.GrainDirectoryName)]
public sealed class DeviceGrain : Grain, IDeviceGrain
{
    private readonly ILogger<DeviceGrain> _logger;

    private readonly IPersistentState<DeviceState> _state;

    private readonly string _name;

    public DeviceGrain(
        ILogger<DeviceGrain> logger,
        [PersistentState(nameof(DeviceState))] IPersistentState<DeviceState> state)
    {
        _logger = logger;
        _state  = state;
        _name   = this.GetPrimaryKeyString();
    }

    public async Task ReceiveHeartbeat()
    {
        _logger.LogInformation("Device({name}) received a heartbeat", _name);

        _state.State.LastHeartbeat = DateTime.UtcNow;
        _state.State.HeartbeatCount++;

        await _state.WriteStateAsync();
    }
}
