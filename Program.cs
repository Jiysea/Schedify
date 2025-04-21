using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Schedify.Configuration;
using Schedify.Data;
using Schedify.Hubs;
using Schedify.Models;
using Schedify.Services;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Stripe configuration
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Hosted Service for Updating Open Events to Ongoing
// builder.Services.AddHostedService<EventStatusUpdater>();

// Add Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
 {
     options.Password.RequiredLength = 10;
     options.Password.RequireNonAlphanumeric = false;
     options.User.RequireUniqueEmail = true;
 }
).AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

    options.SlidingExpiration = true;

    options.LoginPath = "/login";

    options.AccessDeniedPath = "/login"; // Change this to your desired page

    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Headers["HX-Request"] == "true")
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden; // Or 401
            context.Response.Headers["HX-Redirect"] = context.RedirectUri;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Prevents JavaScript access
    options.Cookie.IsEssential = true; // Ensures session is stored even without consent
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ResourceService>();
builder.Services.AddScoped<StripeService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<Schedify.Services.EventService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// async Task CreateAdmin(UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
// {

//     if (!await roleManager.RoleExistsAsync("Admin"))
//     {
//         await roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
//     }

//     // Create an admin user
//     var adminEmail = "admin@gmail.com";
//     var adminUser = await userManager.FindByEmailAsync(adminEmail);

//     if (adminUser == null)
//     {
//         var user = new User
//         {
//             FirstName = "Vincent",
//             MiddleName = "Van",
//             LastName = "Gogh",
//             ExtensionName = null,
//             Birthdate = DateTime.UtcNow,
//             Email = adminEmail,
//             UserName = adminEmail,
//             PhoneNumber = "+639123456789",
//         };

//         var result = await userManager.CreateAsync(user, "Password123"); // Change password

//         if (result.Succeeded)
//         {
//             await userManager.AddToRoleAsync(user, "Admin");
//         }
//     }
// }

// using (var scope = app.Services.CreateScope())
// {
//     var services = scope.ServiceProvider;
//     var userManager = services.GetRequiredService<UserManager<User>>();
//     var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

//     CreateAdmin(userManager, roleManager).Wait();
// }

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();

    if (context.User.Identity?.IsAuthenticated ?? false)
    {
        var user = context.User;


        if (!string.IsNullOrEmpty(path))
        {
            if (path.StartsWith("/admin") && !user.IsInRole("Admin"))
            {
                RedirectToUserHome(context, user);
                return;
            }
            else if (path.StartsWith("/organizer") && !user.IsInRole("Organizer"))
            {
                RedirectToUserHome(context, user);
                return;
            }
            else if (path.StartsWith("/attendee") && !user.IsInRole("Attendee"))
            {
                RedirectToUserHome(context, user);
                return;
            }
            else if (path.StartsWith("/login"))
            {
                RedirectToUserHome(context, user);
                return;
            }
            else if (path.StartsWith("/register"))
            {
                RedirectToUserHome(context, user);
                return;
            }
        }
    }
    else
    {
        if (!string.IsNullOrEmpty(path))
        {
            if (!(path.StartsWith("/login") || path.StartsWith("/register") || path.StartsWith("/")))
            {

                if (context.Request.Headers["HX-Request"] == "true")
                {
                    // HTMX request: Send HX-Redirect header
                    context.Response.Headers.Append("HX-Redirect", "/login");
                    return;
                }
                else
                {
                    // Normal request: Redirect to login page
                    context.Response.Redirect("/login");
                    return;
                }
            }
        }
    }

    await next();
});

// Function to redirect user to their respective homepage
void RedirectToUserHome(HttpContext context, ClaimsPrincipal user)
{
    if (user.IsInRole("Admin"))
    {
        context.Response.Redirect("/admin/dashboard");
    }
    else if (user.IsInRole("Organizer"))
    {
        context.Response.Redirect("/organizer/events");
    }
    else if (user.IsInRole("Attendee"))
    {
        context.Response.Redirect("/attendee/events");
    }
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.MapHub<AlertHub>("/alerts");

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// app.MapHub<ChatHub>("/chatHub");
app.Run();
