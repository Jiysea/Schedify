using Schedify.Data;
using Schedify.Models;
using Schedify.ViewModels;

namespace Schedify.Services;

public class EventService
{
    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;
    private readonly IWebHostEnvironment _environment;

    public EventService(ApplicationDbContext context, UserService userService, IWebHostEnvironment environment)
    {
        _context = context;
        _userService = userService;
        _environment = environment;
    }

    public List<Event> GetEventsByOrganizerDraft(DateTime? startDate, DateTime? endDate)
    {
        var userIdString = _userService.GetUserId();

        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return new List<Event>(); // Return empty list instead of throwing
        }

        var eventsQuery = _context.Events.AsQueryable();

        if (startDate.HasValue)
        {
            eventsQuery = eventsQuery.Where(e => e.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            eventsQuery = eventsQuery.Where(e => e.CreatedAt <= endDate.Value);
        }

        return eventsQuery
            .Where(r => r.UserId == userId && r.Status == EventStatus.Draft)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    public List<Event> GetEventsByOrganizerPublished(DateTime? startDate, DateTime? endDate)
    {
        var userIdString = _userService.GetUserId();

        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return new List<Event>(); // Return empty list instead of throwing
        }

        var eventsQuery = _context.Events.AsQueryable();

        if (startDate.HasValue)
        {
            eventsQuery = eventsQuery.Where(e => e.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            eventsQuery = eventsQuery.Where(e => e.CreatedAt <= endDate.Value);
        }

        return eventsQuery
            .Where(r => r.UserId == userId && (r.Status == EventStatus.Open || r.Status == EventStatus.Ongoing))
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    public List<Event> GetEventsByOrganizerConcluded(DateTime? startDate, DateTime? endDate)
    {
        var userIdString = _userService.GetUserId();

        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return new List<Event>(); // Return empty list instead of throwing
        }

        var eventsQuery = _context.Events.AsQueryable();

        if (startDate.HasValue)
        {
            eventsQuery = eventsQuery.Where(e => e.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            eventsQuery = eventsQuery.Where(e => e.CreatedAt <= endDate.Value);
        }

        return eventsQuery
            .Where(r => r.UserId == userId && (r.Status == EventStatus.Completed || r.Status == EventStatus.Cancelled || r.Status == EventStatus.Postponed))
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error, Event? Event)> CreateEventAsync(CreateEventViewModel model)
    {
        // var validationErrors = ValidateResourceModel(model);
        // if (validationErrors.Any())
        // {
        //     return (false, validationErrors, null);
        // }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _userService.GetUserAsync();
            if (user == null)
            {
                return (false, new Dictionary<string, string> { { "Authentication", "User must be authenticated." } }, null);
            }

            var _event = new Event
            {
                UserId = user.Id,
                Name = model.Name,
                Description = model.Description,
                StartAt = model.StartAt,
                EndAt = model.EndAt,
                Status = model.Status,
                EntryFee = model.EntryFeeDecimal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Events.Add(_event);
            await _context.SaveChangesAsync();

            var conversation = new Conversation
            {
                EventId = _event.Id,
                Title = _event.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return (true, null, _event);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, new Dictionary<string, string> { { "Exception", $"An error occurred: {ex.Message}" } }, null);
        }
    }
}