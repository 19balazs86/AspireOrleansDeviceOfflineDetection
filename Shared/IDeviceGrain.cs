namespace Shared;

public interface IDeviceGrain : IGrainWithStringKey
{
    Task ReceiveHeartbeat();
}
