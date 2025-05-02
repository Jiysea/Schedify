using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schedify.Data;
using Schedify.Models;
using Schedify.ViewModels;
using Stripe;
using Stripe.Checkout;

namespace Schedify.Controllers;

public class PaymentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PaymentController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    [Authorize(Roles = "Attendee")]
    [HttpGet("payment/success")]
    public async Task<IActionResult> Success([FromQuery] string SessionId)
    {
        if (string.IsNullOrEmpty(SessionId))
            return RedirectToAction("Events", "Attendee");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existingPayment = await _context.Payments
                .Include(p => p.EventBooking)
                .FirstOrDefaultAsync(p => p.SessionId == SessionId);

            if (existingPayment == null || existingPayment.Status == "Paid")
                return RedirectToAction("Bookings", "Attendee");

            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(SessionId, new SessionGetOptions
            {
                Expand = new List<string> { "payment_intent", "customer", "payment_intent.payment_method", "line_items.data.price.product" }
            });

            var paymentIntent = session.PaymentIntent;
            var paymentMethod = paymentIntent?.PaymentMethod;
            var card = paymentMethod!.Card;

            // Update Payment
            existingPayment.PaymentMethod = paymentMethod.Type;
            existingPayment.CardBrand = card.Brand;
            existingPayment.PANLastDigits = card.Last4;
            existingPayment.Status = "Paid";
            await _context.SaveChangesAsync();

            // Mark EventBooking as Paid
            existingPayment.EventBooking.Status = BookingStatus.Paid;
            existingPayment.EventBooking.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Find Conversation
            var conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.EventId == existingPayment.EventBooking.EventId);
            if (conversation == null)
                return RedirectToAction("Bookings", "Attendee");

            var userConversation = new ConversationUser
            {
                ConversationId = conversation.Id,
                UserId = existingPayment.UserId,
                JoinedAt = DateTime.UtcNow
            };

            _context.ConversationUsers.Add(userConversation);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var viewModel = new ReceiptViewModel
            {
                ReferenceNumber = session.Id,
                PaymentMethod = paymentMethod!.Type, // e.g. CARD
                CardBrand = card!.Brand,
                Last4 = card.Last4,
                Amount = (decimal)(session.AmountTotal! / 100m),
                EventShortName = (session.LineItems.Data.FirstOrDefault()?.Price?.Product)?.Name,
                CreatedAt = session.Created.ToLocalTime()
            };

            var baseUrl = $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}";
            var plainBytes = Encoding.UTF8.GetBytes(existingPayment.EventBookingId.ToString());

            ViewBag.QRContent = $"{baseUrl}/payment/verify-ticket?token={Convert.ToBase64String(plainBytes)}";
            return View(viewModel);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await transaction.RollbackAsync();
            return RedirectToAction("Events", "Attendee");
        }
    }

    [Authorize(Roles = "Attendee")]
    [HttpGet("payment/view-ticket")]
    public async Task<IActionResult> ViewTicket([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Bookings", "Attendee");

        // 1. Decode Base64 token to get the GUID string
        var base64Bytes = Convert.FromBase64String(token);
        var guidString = Encoding.UTF8.GetString(base64Bytes);

        Guid eventBookingId = Guid.Parse(guidString);

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.EventBookingId == eventBookingId);

        if (payment == null) return RedirectToAction("Bookings", "Attendee");

        var viewModel = new ReceiptViewModel
        {
            ReferenceNumber = payment.SessionId,
            PaymentMethod = payment!.PaymentMethod, // e.g. CARD
            CardBrand = payment!.CardBrand,
            Last4 = payment.PANLastDigits,
            Amount = payment.Amount,
            EventShortName = payment.EventShortName + " (Event Ticket)",
            CreatedAt = payment.CreatedAt.ToLocalTime()
        };

        var baseUrl = $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}";
        ViewBag.QRContent = $"{baseUrl}/payment/verify-ticket?token={token}";
        return View(viewModel);

    }

    [Authorize(Roles = "Organizer")]
    [HttpGet("payment/verify-ticket")]
    public async Task<IActionResult> VerifyTicket(string token)
    {
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Index", "Home");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Decode Base64 token to get the GUID string
            var base64Bytes = Convert.FromBase64String(token);
            var guidString = Encoding.UTF8.GetString(base64Bytes);
            Console.WriteLine("It Works");
            Guid eventBookingId = Guid.Parse(guidString);

            // 2. Fetch the EventBooking including related Event
            var booking = _context.EventBookings
                .Include(b => b.Event)
                .FirstOrDefault(b => b.Id == eventBookingId);

            if (booking == null || booking.Event == null)
                return RedirectToAction("Index", "Home"); // Booking not found
            Console.WriteLine("Booking Found!");

            var evt = booking.Event;

            // 3. Verify if the currently logged-in user is the organizer of the event
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (evt.UserId.ToString() != currentUserId)
                return RedirectToAction("Index", "Home"); // Unauthorized access
            Console.WriteLine("Authorized Organizer Checked!");

            // 4. Check if the event is currently ongoing
            if (evt.Status != EventStatus.Ongoing)
                return RedirectToAction("Denied", "Payment");
            Console.WriteLine("Event is ongoing, Check!");

            // 5. Check if the ticket is already used
            if (booking.Status != BookingStatus.Paid)
                return RedirectToAction("Denied", "Payment");
            Console.WriteLine("Ticket is Paid, Check!");

            // 6. Update status of booking and its payment
            booking.Status = BookingStatus.Confirmed;
            var payment = _context.Payments.FirstOrDefault(p => p.EventBookingId == booking.Id);
            if (payment != null)
            {
                payment.Status = "Confirmed";
            }
            Console.WriteLine("Your ticket is confirmed.");

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // 6. Redirect to appropriate home page
            return RedirectToAction("TicketSecured", "Payment");

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await transaction.RollbackAsync();
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet("payment/cancel")]
    public async Task<IActionResult> CancelAsync(string SessionId)
    {
        if (string.IsNullOrEmpty(SessionId))
            return RedirectToAction("Events", "Attendee");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existingPayment = await _context.Payments.FirstOrDefaultAsync(p => p.SessionId == SessionId);
            if (existingPayment == null)
                return RedirectToAction("Events", "Attendee");

            var existingBooking = await _context.EventBookings.FindAsync(existingPayment.EventBookingId);
            if (existingBooking == null)
                return RedirectToAction("Events", "Attendee");

            _context.Payments.Remove(existingPayment);
            _context.EventBookings.Remove(existingBooking);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return View();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await transaction.RollbackAsync();
            return RedirectToAction("Events", "Attendee");
        }
    }

    public IActionResult Denied()
    {
        return View();
    }

    public IActionResult TicketSecured()
    {
        return View();
    }

}

