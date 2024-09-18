using Microsoft.AspNetCore.SignalR;
using Orleans.GrainDirectory;
using OrleansServer.GrainStates;
using OrleansServer.Hubs;
using Shared;

namespace OrleansServer.Grains;

[GrainDirectory(Constants.GrainDirectoryName)]
public sealed class DeviceGrain : Grain, IDeviceGrain
{
    private const string _offlineReminderName = "Offline";
    private const string _deleteReminderName  = "Delete";

    private readonly ILogger<DeviceGrain> _logger;

    private readonly IPersistentState<DeviceState> _state;

    private readonly IHubContext<DeviceHub, IDeviceHubClient> _deviceHub;

    private readonly string _deviceId;

    private IGrainReminder? _offlineReminder;
    private IGrainReminder? _deleteReminder;

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

        if (_deleteReminder is not null)
        {
            await this.UnregisterReminder(_deleteReminder);

            _deleteReminder = null;
        }

        _offlineReminder = await this.RegisterOrUpdateReminder(_offlineReminderName, dueTime: TimeSpan.FromSeconds(30), period: TimeSpan.FromMinutes(1));

        await notifyClients("online");
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _offlineReminder = await this.GetReminder(_offlineReminderName);
        _deleteReminder  = await this.GetReminder(_deleteReminderName);
    }

    public async Task ReceiveReminder(string reminderName, TickStatus status)
    {
        _logger.LogInformation("Received reminder: {ReminderName}", reminderName);

        if (_offlineReminderName.Equals(reminderName))
        {
            await handleOffline();
        }
        else if (_deleteReminderName.Equals(reminderName))
        {
            await handleDelete();
        }
        else
        {
            throw new NotSupportedException($"Reminder('{reminderName}') is not supported");
        }
    }

    private async Task handleOffline()
    {
        if (_offlineReminder is not null)
        {
            await this.UnregisterReminder(_offlineReminder);

            _offlineReminder = null;
        }

        _deleteReminder = await this.RegisterOrUpdateReminder(_deleteReminderName, dueTime: TimeSpan.FromSeconds(60), period: TimeSpan.FromMinutes(1));

        await notifyClients("offline");
    }

    private async Task handleDelete()
    {
        if (_deleteReminder is not null)
        {
            await this.UnregisterReminder(_deleteReminder);

            _deleteReminder = null;
        }

        await _state.ClearStateAsync(); // Note: AzureTableStorageOptions.DeleteStateOnClear is set to true
    }

    private async Task notifyClients(string status)
    {
        await _deviceHub.Clients.All.NotifyStatusChanged(_deviceId, status);
    }
}
