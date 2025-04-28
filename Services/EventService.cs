using Microsoft.AspNetCore.Mvc;
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

    public async Task<List<Event>?> GetEventsByUser()
    {
        return await _context.Events
            .Where(e => e.UserId == Guid.Parse(_userService.GetUserId()!))
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public Event? GetEventById(Guid Id)
    {
        return _context.Events
            .Where(e => e.UserId == Guid.Parse(_userService.GetUserId()!) && e.Id == Id)
            .FirstOrDefault();
    }

    public async Task<Event?> GetEventByIdAsync(Guid Id)
    {
        return await _context.Events
            .Where(e => e.Id == Id)
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<Guid, bool>> GetEventHasVenueByUser()
    {
        return await _context.Events
            .Include(e => e.Resources)
            .Where(e => e.UserId == Guid.Parse(_userService.GetUserId()!))
            .ToDictionaryAsync(
                e => e.Id,
                e => e.Resources.Any(r => r.ResourceType == ResourceType.Venue));
    }

    public async Task<bool> IsEventHasVenue(Guid EventId)
    {
        var resources = await _context.Resources.Where(er => er.EventId == EventId).ToListAsync();

        foreach (var resource in resources)
        {
            if (resource.ResourceType == ResourceType.Venue) return true;
        }

        return false;
    }

    public async Task<Dictionary<Guid, bool>> GetEventIsOpenableByUser()
    {
        return await _context.Events
            .Include(e => e.Resources)
            .Where(e => e.UserId == Guid.Parse(_userService.GetUserId()!))
            .ToDictionaryAsync(
                e => e.Id,
                e => e.StartAt >= DateTime.UtcNow);
    }

    public async Task<bool> IsEventOpenable(Guid EventId)
    {
        var evt = await _context.Events.FindAsync(EventId);
        if (evt == null) return false;
        if (evt.StartAt <= DateTime.UtcNow) return false;
        return true;
    }

    public async Task<List<Event>> GetEventsForDropdown()
    {
        var events = await _context.Events
            .Where(e => e.UserId == Guid.Parse(_userService.GetUserId()!))
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return events;
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

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error, Event? Event)> CreateEventAsync(CUEventViewModel model)
    {

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
                ShortName = model.ShortName,
                Description = model.Description,
                StartAt = model.StartAt,
                EndAt = model.EndAt,
                TimeStart = model.TimeStart,
                TimeEnd = model.TimeEnd,
                Status = model.Status,
                EntryFee = model.EntryFee,
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

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error, Event? Event)> UpdateEventAsync(CUEventViewModel model)
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

            if (model.Status != EventStatus.Draft || _event.Status != EventStatus.Draft)
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

    public async Task<bool> DeleteEventAsync(Guid Id)
    {
        var _event = await _context.Events
            .Include(e => e.Conversation)
            .FirstOrDefaultAsync(r => r.Id == Id);

        if (_event == null) return false;

        var resources = _context.Resources.Where(r => r.EventId == _event.Id)
            .Include(r => r.Image)
            .Include(r => r.ResourceVenue)
            .Include(r => r.ResourceEquipment)
            .Include(r => r.ResourceFurniture)
            .Include(r => r.ResourceCatering)
            .Include(r => r.ResourcePersonnel)
            .ToList();

        if (_event.Status != EventStatus.Draft)
        {
            return false;
        }

        // Only Delete Resources when there is at least one
        if (resources != null)
        {
            if (resources.Count > 0)
            {

                foreach (var resource in resources)
                {
                    // Delete the associated resource type model
                    switch (resource.ResourceType)
                    {
                        case ResourceType.Venue:
                            if (resource.ResourceVenue != null) _context.ResourceVenues.Remove(resource.ResourceVenue);
                            break;

                        case ResourceType.Equipment:
                            if (resource.ResourceEquipment != null) _context.ResourceEquipments.Remove(resource.ResourceEquipment);
                            break;

                        case ResourceType.Furniture:
                            if (resource.ResourceFurniture != null) _context.ResourceFurnitures.Remove(resource.ResourceFurniture);
                            break;

                        case ResourceType.Catering:
                            if (resource.ResourceCatering != null) _context.ResourceCaterings.Remove(resource.ResourceCatering);
                            break;

                        case ResourceType.Personnel:
                            if (resource.ResourcePersonnel != null) _context.ResourcePersonnels.Remove(resource.ResourcePersonnel);
                            break;
                    }

                    // Delete associated images first
                    if (resource.Image?.ImageFileName != null)
                    {
                        string imagePath = Path.Combine(_environment.WebRootPath, "resources", resource.Image.ImageFileName);
                        if (File.Exists(imagePath)) File.Delete(imagePath);
                        _context.Images.Remove(resource.Image);
                    }

                    _context.Resources.Remove(resource);
                    await _context.SaveChangesAsync();
                }
            }
        }

        // Delete associated conversations first
        _context.Conversations.Remove(_event.Conversation);

        _context.Events.Remove(_event);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error)> OpenEventByIdAsync(Guid Id)
    {
        var _event = await _context.Events
            .FindAsync(Id);

        if (_event == null) return (false, new Dictionary<string, string> { { "NullEvent", "Event does not exist." } });

        if (_event.Status != EventStatus.Draft) return (false, new Dictionary<string, string> { { "Status", "Event should be in draft phase." } });

        if (_event.StartAt <= DateTime.UtcNow) return (false, new Dictionary<string, string> { { "DateOvershoot", "Event schedule has already passed." } });

        _event.Status = EventStatus.Open;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error)> DraftEventByIdAsync(Guid Id)
    {
        var _event = await _context.Events
            .FindAsync(Id);

        if (_event == null) return (false, new Dictionary<string, string> { { "NullEvent", "Event does not exist." } });

        _event.Status = EventStatus.Draft;
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error)> CancelEventByIdAsync(Guid Id)
    {
        var _event = await _context.Events
            .FindAsync(Id);

        if (_event == null) return (false, new Dictionary<string, string> { { "NullEvent", "Event does not exist." } });

        if (_event.Status == EventStatus.Open || _event.Status == EventStatus.Ongoing || _event.Status == EventStatus.Postponed)
        {
            _event.Status = EventStatus.Cancelled;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        return (false, new Dictionary<string, string> { { "Unsuccessful", "Could not cancel the event." } });
    }

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error, Event? Event)> PostponeEventByIdAsync(CUEventViewModel model)
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
                return (false, new Dictionary<string, string> { { "NotFound", "Event does not exist." } }, null);
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

    // Attendees
    public List<Event> GetOpenedEvents(DateTime? startDate, DateTime? endDate)
    {
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
            .Include(e => e.User)
            .Include(e => e.Resources)
                .ThenInclude(e => e.ResourceVenue)
            .Where(e => e.Status == EventStatus.Open)
            .OrderByDescending(e => e.UpdatedAt)
            .ToList();
    }

    public Dictionary<Guid, string?> GetEventVenueImages(List<Event> events)
    {
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
        var images = _context.Images
            .Where(img => img.ResourceId.HasValue && venueResourceIds.Contains(img.ResourceId.Value))
            .ToList();

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

    public Dictionary<Guid, string?> GetOrganizerImages(List<Event> events)
    {
        var organizerIds = events
            .Select(u => u.User!.Id)
            .ToList();

        // Get images for those user IDs
        return organizerIds
            .ToDictionary(
                organizerId => organizerId,
                organizerId => _context.Images
                    .Where(img => img.UserId == organizerId)
                    .Select(img => img.ImageFileName)
                    .FirstOrDefault() // returns null if not found
            );
    }

}