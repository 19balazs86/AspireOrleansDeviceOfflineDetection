namespace Shared;

public interface IDeviceGrain : IGrainWithStringKey, IRemindable
{
    Task ReceiveHeartbeat();
}
