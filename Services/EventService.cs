using Microsoft.EntityFrameworkCore;
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
            .Where(r => r.UserId == userId && (r.Status == EventStatus.Open || r.Status == EventStatus.Ongoing || r.Status == EventStatus.Postponed))
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
            .Where(r => r.UserId == userId && (r.Status == EventStatus.Completed || r.Status == EventStatus.Cancelled))
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    public Dictionary<Guid, int> GetAttendeeCounts(List<Event> events)
    {
        var eventIds = events.Select(e => e.Id).ToList();

        return _context.EventBookings
            .Where(eb => eventIds.Contains(eb.EventId))
            .GroupBy(eb => eb.EventId)
            .Select(g => new { EventId = g.Key, Count = g.Count() })
            .ToDictionary(g => g.EventId, g => g.Count);
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

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error, Event? Event)> UpdateEventAsync(UpdateEventViewModel model)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _userService.GetUserAsync();
            if (user == null)
            {
                return (false, new Dictionary<string, string> { { "Authentication", "User must be authenticated." } }, null);
            }

            if (model.Status != EventStatus.Draft)
            {
                return (false, new Dictionary<string, string> { { "InvalidStatus", "The event status should be on \"Draft\" before updating." } }, null);
            }

            var _event = await _context.Events.FindAsync(model.Id);

            if (_event == null)
            {
                return (false, new Dictionary<string, string> { { "NotFound", "Event not found." } }, null);
            }

            if (_event.Status != EventStatus.Draft)
            {
                return (false, new Dictionary<string, string> { { "InvalidStatus", "The event status should be on \"Draft\" before updating." } }, null);
            }

            _context.Entry(_event).CurrentValues.SetValues(model);

            if (!_context.ChangeTracker.HasChanges())
            {
                return (false, new Dictionary<string, string> { { "NoChanges", "Seems like there's no need to change." } }, null);
            }

            _event.UpdatedAt = DateTime.UtcNow;

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

    public async Task<ViewEventViewModel?> GetEventByIdAsync(Guid Id)
    {
        var _event = await _context.Events
            .Include(e => e.Conversation)
            .FirstOrDefaultAsync(e => e.Id == Id);

        if (_event == null) return null;

        bool hasVenue = IsEventHasVenue(_event.Id);

        return new ViewEventViewModel()
        {
            Id = _event.Id,
            Name = _event.Name,
            Description = _event.Description,
            StartAt = _event.StartAt,
            EndAt = _event.EndAt,
            Status = _event.Status,
            EntryFee = _event.EntryFee.ToString("N2"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EventHasVenue = hasVenue,
        };
    }

    public bool IsEventHasVenue(Guid Id)
    {
        var eventResources = _context.EventResources.Include(er => er.Resource).Where(er => er.EventId == Id).ToList();

        foreach (var eventResource in eventResources)
        {
            if (eventResource.Resource.Type == ResourceType.Venue)
            {
                return true;
            }
        }

        return false;
    }

    public async Task<UpdateEventViewModel?> GetEditEventByIdAsync(Guid Id)
    {
        var _event = await _context.Events
            .Include(e => e.Conversation)
            .FirstOrDefaultAsync(e => e.Id == Id);

        if (_event == null) return null;

        return new UpdateEventViewModel()
        {
            Id = _event.Id,
            Name = _event.Name,
            Description = _event.Description,
            StartAt = _event.StartAt,
            EndAt = _event.EndAt,
            Status = _event.Status,
            EntryFeeString = _event.EntryFee.ToString("N2"),
        };
    }

    public async Task<bool> DeleteEventAsync(Guid Id)
    {
        var _event = await _context.Events
            .Include(e => e.Conversation)
            .FirstOrDefaultAsync(r => r.Id == Id);

        if (_event == null) return false;

        if (_event.Status != EventStatus.Draft)
        {
            return false;
        }

        // Delete associated conversations first
        _context.Conversations.Remove(_event.Conversation);

        _context.Events.Remove(_event);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> OpenEventByIdAsync(Guid Id)
    {
        Console.WriteLine(Id);
        Console.WriteLine(Id);
        Console.WriteLine(Id);
        Console.WriteLine(Id);
        var _event = await _context.Events
            .FindAsync(Id);

        if (_event == null) return false;

        if (_event.Status != EventStatus.Draft)
        {
            return false;
        }

        _event.Status = EventStatus.Open;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DraftEventByIdAsync(Guid Id)
    {
        var _event = await _context.Events
            .FindAsync(Id);

        if (_event == null) return false;

        _event.Status = EventStatus.Draft;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelEventByIdAsync(Guid Id)
    {
        var _event = await _context.Events
            .FindAsync(Id);

        if (_event == null) return false;

        if (_event.Status == EventStatus.Open || _event.Status == EventStatus.Ongoing || _event.Status == EventStatus.Postponed)
        {
            _event.Status = EventStatus.Cancelled;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error, Event? Event)> PostponeEventByIdAsync(UpdateEventViewModel model)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _userService.GetUserAsync();
            if (user == null)
            {
                return (false, new Dictionary<string, string> { { "Authentication", "User must be authenticated." } }, null);
            }

            var _event = await _context.Events.FindAsync(model.Id);

            if (_event == null)
            {
                return (false, new Dictionary<string, string> { { "NotFound", "Event not found." } }, null);
            }

            if (_event.StartAt == model.StartAt)
            {
                return (false, new Dictionary<string, string> { { "NoChanges", "You need to set a new event duration to postpone the event." } }, null);
            }

            if (_event.Status == EventStatus.Open || _event.Status == EventStatus.Ongoing)
            {
                _event.Status = EventStatus.Postponed;
                _event.StartAt = model.StartAt;
                _event.EndAt = model.EndAt;
                _event.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return (true, null, _event);
            }

            return (false, new Dictionary<string, string> { { "InvalidStatus", "The event status should be on \"Open\" or \"Onoing\" before updating." } }, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, new Dictionary<string, string> { { "Exception", $"An error occurred: {ex.Message}" } }, null);
        }
    }

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error, EventResource? EventResource)> AddToEventResourceAsync(EventResourceViewModel model)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _userService.GetUserAsync();
            if (user == null)
            {
                return (false, new Dictionary<string, string> { { "Authentication", "User must be authenticated." } }, null);
            }

            var resource = await _context.Resources.FindAsync(model.ResourceId);

            if (resource == null) return (false, new Dictionary<string, string> { { "NullError", "Resource not found." } }, null);
            
            resource.Quantity -= model.QuantityFromForm;
            resource.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            var eventResource = new EventResource
            {
                ResourceId = resource.Id,
                EventId = model.EventId,
                TotalCost = model.TotalCost,
                Quantity = model.QuantityFromForm,
                AddedAt = DateTime.UtcNow,
            };

            _context.EventResources.Add(eventResource);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return (true, null, eventResource);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, new Dictionary<string, string> { { "Exception", $"An error occurred: {ex.Message}" } }, null);
        }
    }

}