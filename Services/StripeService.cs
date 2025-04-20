using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schedify.Data;
using Schedify.Models;
using Schedify.ViewModels;
using Stripe;
using Stripe.Checkout;

namespace Schedify.Services;

public class StripeService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserService _userService;

    public StripeService(ApplicationDbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, UserService userService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        _userService = userService;
    }

    public async Task<PaymentIntent?> CreatePaymentIntentAsync(long amount, string currency)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount * 100, // e.g. 5000 = ₱50.00
                Currency = currency, // "php"
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            var service = new PaymentIntentService();
            return await service.CreateAsync(options);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public async Task<string> CreateCheckoutSessionAsync(CheckoutViewModel booking, Guid EventId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var baseUrl = $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                SuccessUrl = $"{baseUrl}/payment/success?SessionId={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{baseUrl}/payment/cancel?SessionId={{CHECKOUT_SESSION_ID}}",
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Quantity = booking.Quantity,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = booking.Currency,
                        UnitAmount = booking.Amount * 100, // In cents (₱999.00 => 99900)
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = booking.EventTitle + " (Event Ticket)"
                        }
                    }
                }
            }
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            var user = await _userService.GetUserAsync();
            if (user == null) throw new Exception();

            var latestBooking = await _context.EventBookings
                .Where(b => b.UserId == user.Id && b.EventId == EventId)
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync();

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                EventBookingId = latestBooking!.Id,
                SessionId = session.Id,
                Amount = latestBooking.TotalPrice,
                PaymentMethod = "Pending",
                CardBrand = "Pending",
                PANLastDigits = "----",
                EventShortName = booking.EventTitle,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            
            await transaction.CommitAsync();

            return session.Url; // Redirect the user to this URL
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await transaction.RollbackAsync();
            return string.Empty;
        }
    }
}