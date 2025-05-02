using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Schedify.Hubs;
using Schedify.Services;
using Schedify.ViewModels;

namespace Schedify.Controllers;

[Authorize]
public class ChatController : Controller
{

    private readonly ChatService _chatService;
    private readonly IHubContext<ChatHub> _chatHub;

    public ChatController(ChatService chatService, IHubContext<ChatHub> chatHub)
    {
        _chatService = chatService;
        _chatHub = chatHub;
    }

    [HttpGet("chat/conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var Conversations = await _chatService.GetConversationsAsync();
        var EventImages = await _chatService.GetEventImagesAsync(Conversations);

        var viewModel = new ChatViewModel
        {
            Conversations = Conversations,
            EventImages = EventImages
        };

        return PartialView("~/Views/Chat/Partials/LoadChat.cshtml", viewModel);
    }   

    [HttpPost("chat/send")]
    public async Task<IActionResult> SendMessage(Guid conversationId, Guid userId, string content)
    {
        var message = await _chatService.SendMessageAsync(conversationId, userId, content);
        await _chatHub.Clients.Group(conversationId.ToString())
            .SendAsync("ReceiveMessage", userId, content, message.CreatedAt);
        return Ok(message);
    }

    [HttpGet("chat/messages/{conversationId}")]
    public async Task<IActionResult> GetMessages(Guid conversationId)
    {
        var messages = await _chatService.GetMessagesAsync(conversationId);
        return Ok(messages);
    }

    [HttpPut("chat/edit/{messageId}")]
    public async Task<IActionResult> EditMessage(Guid messageId, string newContent)
    {
        var result = await _chatService.EditMessageAsync(messageId, newContent);
        return result ? Ok() : NotFound();
    }

    [HttpDelete("chat/delete/{messageId}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {
        var result = await _chatService.DeleteMessageAsync(messageId);
        return result ? Ok() : NotFound();
    }
}

