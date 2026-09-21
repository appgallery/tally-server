using Microsoft.AspNetCore.SignalR;

namespace TallyServer.Hubs;

public class TallyHub : Hub
{
    private static readonly Dictionary<string, string> Devices
        = new();

    private static readonly object LockObject = new();

    public override async Task OnConnectedAsync()
    {
        var deviceId =
            Context.GetHttpContext()?
                .Request.Query["deviceId"]
                .ToString();

        if (!string.IsNullOrWhiteSpace(deviceId))
        {
            lock (LockObject)
            {
                Devices[deviceId] =
                    Context.ConnectionId;
            }

            Console.WriteLine(
                $"Tally connector connected: {deviceId}"
            );
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(
        Exception? exception)
    {
        var device =
            Devices.FirstOrDefault(
                x => x.Value == Context.ConnectionId
            );

        if (!string.IsNullOrWhiteSpace(device.Key))
        {
            lock (LockObject)
            {
                Devices.Remove(device.Key);
            }

            Console.WriteLine(
                $"Tally connector disconnected: {device.Key}"
            );
        }

        await base.OnDisconnectedAsync(exception);
    }

    public static string? GetConnectionId(
        string deviceId)
    {
        lock (LockObject)
        {
            Devices.TryGetValue(
                deviceId,
                out var connectionId
            );

            return connectionId;
        }
    }

    public async Task SendMessage(
        string message)
    {
        await Clients.Caller.SendAsync(
            "ReceiveMessage",
            message
        );
    }
}