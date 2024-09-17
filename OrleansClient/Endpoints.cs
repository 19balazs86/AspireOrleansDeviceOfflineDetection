using Shared;

namespace OrleansClient;

public static class Endpoints
{
    public static void MapDeviceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/send-heartbeat", sendHeartbeat);
    }

    private static async Task sendHeartbeat(IClusterClient clusterClient)
    {
        int[] deviceIds = Enumerable.Range(0, 10).Select(_ => Random.Shared.Next(1, 21)).ToArray();

        await Parallel.ForEachAsync(deviceIds, async (deviceId, ct) =>
        {
            IDeviceGrain deviceGrain = clusterClient.GetGrain<IDeviceGrain>(deviceId.ToString());

            await deviceGrain.ReceiveHeartbeat();
        });
    }
}
