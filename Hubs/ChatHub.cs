using Microsoft.AspNetCore.SignalR;

namespace Schedify.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(Guid conversationId, Guid userId, string message)
    {
        await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", userId, message, DateTime.UtcNow);
    }

    public async Task JoinConversation(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
    }

}

