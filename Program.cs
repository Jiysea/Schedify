using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Schedify.Data;
using Schedify.Hubs;
using Schedify.Models;
using Schedify.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ResourceService>();
builder.Services.AddScoped<EventService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

async Task CreateAdmin(UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
{

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole<Guid>("Admin"));
    }

    // Create an admin user
    var adminEmail = "admin@gmail.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var user = new User
        {
            FirstName = "Admin",
            MiddleName = null,
            LastName = "Admin",
            ExtensionName = null,
            Birthdate = DateTime.UtcNow,
            Email = adminEmail,
            UserName = adminEmail,
            PhoneNumber = "09123456789",
        };

        var result = await userManager.CreateAsync(user, "Password123"); // Change password

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    CreateAdmin(userManager, roleManager).Wait();
}

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated ?? false) 
    {
        var user = context.User;
        var path = context.Request.Path.Value?.ToLower();

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


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// app.MapHub<ChatHub>("/chatHub");
app.Run();
