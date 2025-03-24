using System;
using Microsoft.AspNetCore.SignalR;

namespace Schedify.Hubs;


public class AlertHub : Hub
{
    private static readonly Dictionary<string, string> _connections = new();

    public static void SaveConnectionId(string userId, string connectionId)
    {
        _connections[userId] = connectionId; // Save or update user's connection ID
    }

    public static string? GetConnectionId(string userId)
    {
        return _connections.TryGetValue(userId, out var connId) ? connId : null;
    }

    public async Task SendNotification(string message)
    {
        await Clients.Caller.SendAsync("ReceiveNotification", message);
    }
}
