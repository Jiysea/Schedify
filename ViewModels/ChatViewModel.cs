using Schedify.Models;

namespace Schedify.ViewModels;

public class ChatViewModel
{
    public List<Conversation> Conversations = new List<Conversation>();
    public Dictionary<Guid, string?> EventImages { get; set; } = new Dictionary<Guid, string?>();
}