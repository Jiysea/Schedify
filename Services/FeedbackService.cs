using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schedify.Data;
using Schedify.Models;
using Schedify.ViewModels;
using Stripe.Checkout;

namespace Schedify.Services;

public class FeedbackService
{

    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;
    private readonly IWebHostEnvironment _environment;
    private readonly UserManager<User> _userManager;

    public FeedbackService(ApplicationDbContext context, UserService userService, IWebHostEnvironment environment, UserManager<User> userManager)
    {
        _context = context;
        _userService = userService;
        _environment = environment;
        _userManager = userManager;
    }

    public async Task<Feedback?> GetFeedbackByIdAsync(Guid FeedbackId)
    {
        return await _context.Feedbacks.FindAsync(FeedbackId);
    }

    public async Task<Feedback?> GetFeedbackByEventIdAsync(Guid EventId)
    {
        var user = await _userService.GetUserAsync();
        if (user == null) return null;

        return await _context.Feedbacks.FirstOrDefaultAsync(f => f.EventId == EventId && f.UserId == user.Id);
    }

    public async Task<List<Feedback>?> GetFeedbacksByEventIdAsync(Guid EventId)
    {
        return await _context.Feedbacks
            .Where(f => f.EventId == EventId)
            .ToListAsync();
    }

    public async Task<Dictionary<Guid, string?>?> GetAvatarImagesAsync(List<Feedback>? feedbacks)
    {
        var userIds = feedbacks?
            .Select(f => f.UserId)
            .Distinct()
            .ToList();

        var images = await _context.Images
            .Where(i => i.UserId != null && userIds!.Contains(i.UserId.Value))
            .ToDictionaryAsync(i => i.UserId!.Value, i => i.ImageFileName);

        var avatars = feedbacks?.ToDictionary(
            feedback => feedback.Id,
            feedback => images.TryGetValue(feedback.UserId, out var image) ? image : null
            );

        return avatars;
    }

    public async Task<Dictionary<Guid, string?>> GetUserAsync(List<Feedback>? feedbacks)
    {
        if (feedbacks == null || feedbacks.Count == 0)
            return new Dictionary<Guid, string?>();

        var userIds = feedbacks
            .Select(f => f.UserId)
            .Distinct()
            .ToList();

        var usersById = await _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => (User?)u);

        var userMap = feedbacks.ToDictionary(
            feedback => feedback.Id,
            feedback => usersById.TryGetValue(feedback.UserId, out var user) ? GetFullName(user!.FirstName!, user?.MiddleName, user!.LastName!, user?.ExtensionName) : null
        );

        return userMap;
    }

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error, Feedback? Feedback)> CreateFeedbackAsync(FeedbackViewModel model)
    {
        var validationErrors = ValidateResourceModel(model);
        if (validationErrors.Any())
        {
            return (false, validationErrors, null);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _userService.GetUserAsync();
            if (user == null)
            {
                return (false, new Dictionary<string, string> { { "Authentication", "User must be authenticated." } }, null);
            }

            if (model.EventId == null)
            {
                return (false, new Dictionary<string, string> { { "NoEvent", "Event not found." } }, null);
            }

            var booking = _context.EventBookings.FirstOrDefault(e => e.EventId == model.EventId && e.UserId == user.Id);

            if (booking == null)
            {
                return (false, new Dictionary<string, string> { { "Authorization", "Invalid user." } }, null);
            }

            var feedback = new Feedback
            {
                UserId = user.Id,
                EventId = (Guid)model.EventId,
                Rating = model.Rating,
                Comments = model.Comments,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return (true, null, feedback);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, new Dictionary<string, string> { { "Exception", $"An error occurred: {ex.Message}" } }, null);
        }
    }

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error, Feedback? Feedback)> UpdateFeedbackAsync(FeedbackViewModel model)
    {
        var validationErrors = ValidateResourceModel(model);
        if (validationErrors.Any())
        {
            return (false, validationErrors, null);
        }

        var user = await _userService.GetUserAsync();
        if (user == null)
            return (false, new Dictionary<string, string> { { "Authentication", "User must be authenticated." } }, null);


        var feedback = await _context.Feedbacks.FindAsync(model.Id);
        if (feedback == null)
            return (false, new Dictionary<string, string> { { "NoFeedback", "Feedback not found." } }, null);

        var evt = await _context.Events.FindAsync(feedback.EventId);

        if (evt == null)
            return (false, new Dictionary<string, string> { { "NoEvent", "Event not found." } }, null);


        var booking = await _context.EventBookings.FirstOrDefaultAsync(e => e.EventId == feedback.EventId && e.UserId == user.Id);

        if (booking == null)
            return (false, new Dictionary<string, string> { { "Authorization", "Invalid user." } }, null);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {

            feedback.Rating = model.Rating;
            feedback.Comments = model.Comments;
            feedback.UpdatedAt = DateTime.UtcNow;

            _context.Update(feedback);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return (true, null, feedback);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, new Dictionary<string, string> { { "Exception", $"An error occurred: {ex.Message}" } }, null);
        }
    }

    private Dictionary<string, string> ValidateResourceModel(FeedbackViewModel model)
    {
        var errors = new Dictionary<string, string>();

        if (model.Rating == 0)
        {
            errors["NoRating"] = "Please rate 1 to 5.";
        }
        else if ((model.Rating > 5 || model.Rating < 1) && model.Rating != 0)
        {
            errors["InvalidRating"] = "Please rate only within 1 to 5.";
        }

        return errors;
    }

    private string GetFullName(string firstName, string? middleName, string lastName, string? extensionName)
    {
        var parts = new List<string>
        {
            firstName
        };

        if (!string.IsNullOrWhiteSpace(middleName))
            parts.Add(middleName);

        parts.Add(lastName);

        if (!string.IsNullOrWhiteSpace(extensionName))
            parts.Add(extensionName);

        return string.Join(" ", parts);
    }
}