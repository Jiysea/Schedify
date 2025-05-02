using Microsoft.EntityFrameworkCore;
using Schedify.Data;
using Schedify.Models;

namespace Schedify.Services;

public class ChatService
{
    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;
    private readonly IWebHostEnvironment _environment;

    public ChatService(ApplicationDbContext context, UserService userService, IWebHostEnvironment environment)
    {
        _context = context;
        _userService = userService;
        _environment = environment;
    }

    public async Task<List<Conversation>> GetConversationsAsync()
    {
        var userId = Guid.Parse(_userService.GetUserId()!);

        return await _context.Conversations
            .Include(c => c.ConversationUsers)
            .Where(c => c.ConversationUsers.Any(cu => cu.UserId == userId))
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Dictionary<Guid, string?>> GetEventImagesAsync(List<Conversation> conversations)
    {
        var events = await _context.Events
            .Include(e => e.Resources)
            .Where(e => conversations.Select(c => c.EventId).Distinct().Contains(e.Id))
            .ToListAsync();

        // Get (EventId, VenueResourceId)
        var eventVenueResources = events
            .Select(e => new
            {
                EventId = e.Id,
                VenueResourceId = e.Resources.FirstOrDefault(r => r.ResourceType == ResourceType.Venue)?.Id
            })
            .Where(x => x.VenueResourceId.HasValue)
            .ToList();

        var venueResourceIds = eventVenueResources
        .Select(x => x.VenueResourceId!.Value)
        .ToList();

        // Get images for those venue resource IDs
        var images = await _context.Images
            .Where(img => img.ResourceId.HasValue && venueResourceIds.Contains(img.ResourceId.Value))
            .ToListAsync();

        // Match EventId to ImageFileName (can be null if no image found)
        var result = eventVenueResources
            .ToDictionary(
                ev => ev.EventId,
                ev =>
                {
                    var image = images.FirstOrDefault(img => img.ResourceId == ev.VenueResourceId);
                    return image?.ImageFileName;
                });

        return result;
    }

    public async Task<List<Message>> GetMessagesAsync(Guid conversationId)
    {
        return await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<Message> SendMessageAsync(Guid conversationId, Guid userId, string content)
    {
        var message = new Message
        {
            ConversationId = conversationId,
            UserId = userId,
            Content = content
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<bool> EditMessageAsync(Guid messageId, string newContent)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null) return false;

        message.Content = newContent;
        message.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMessageAsync(Guid messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null) return false;

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
        return true;
    }

}