namespace OrleansServer.GrainStates;

[GenerateSerializer]
public sealed class DeviceState
{
    [Id(0)]
    public DateTime LastHeartbeat { get; set; }

    [Id(1)]
    public int HeartbeatCount { get; set; }
}
