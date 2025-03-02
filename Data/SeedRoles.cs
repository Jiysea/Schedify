using Microsoft.AspNetCore.Identity;
using Schedify.Models;

namespace Schedify.Data; // Replace with your actual project namespace
public static class SeedRoles
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

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

            var result = await userManager.CreateAsync(user, "Password123!"); // Change password

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
