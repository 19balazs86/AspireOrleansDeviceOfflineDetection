namespace ServiceDefaults;

public interface IDeviceGrain : IGrainWithStringKey, IRemindable
{
    Task ReceiveHeartbeat();
}
