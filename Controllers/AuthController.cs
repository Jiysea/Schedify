using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Schedify.Data;
using Schedify.Models;
using Schedify.ViewModels;

namespace Schedify.Controllers;

public class AuthController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public AuthController(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager, ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _environment = environment;
    }

    [Route("login")]
    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }

    [Route("auth/login")]
    [HttpPost]
    public async Task<IActionResult> LoginUser(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_ValidationMessages", ModelState);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            ModelState.AddModelError("Email", "User not found.");
            return PartialView("_ValidationMessages", ModelState);
        }
        var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return PartialView("_ValidationMessages", ModelState);
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Contains("Admin"))
            Response.Headers.Append("HX-Redirect", Url.Action("Resources", "Admin"));
        else if (roles.Contains("Organizer"))
            Response.Headers.Append("HX-Redirect", Url.Action("Events", "Organizer"));
        else if (roles.Contains("Attendee"))
            Response.Headers.Append("HX-Redirect", Url.Action("Events", "Attendee"));

        return Content(string.Empty);
    }

    [Route("register")]
    [HttpGet]
    public ActionResult Register()
    {
        return View();
    }

    [Route("auth/register")]
    [HttpPost]
    public async Task<IActionResult> RegisterUser(RegisterViewModel model)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Phone Number custom validation
            if (model.PhoneNumber.Length != 10)
            {
                ModelState.AddModelError("PhoneNumber", "Value must be 10 digits.");
            }
            else if (!model.PhoneNumber.StartsWith("9"))
            {
                ModelState.AddModelError("PhoneNumber", "Value must start with 9.");
            }
            else if (!model.PhoneNumber.All(char.IsDigit))
            {
                ModelState.AddModelError("PhoneNumber", "Value must be a number.");
            }

            if (!ModelState.IsValid)
            {
                return PartialView("_ValidationMessages", ModelState);
            }

            var PhoneNumber = string.Concat(["+63", model.PhoneNumber]);

            User user = new User
            {
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                ExtensionName = model.ExtensionName,
                Birthdate = model.Birthdate,
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Password", error.Description); // Store password-related errors
                }
                return PartialView("_ValidationMessages", ModelState);
            }

            if (model.ImageFile != null)
            {
                string newFileName = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + Path.GetExtension(model.ImageFile.FileName);
                string imageFullPath = Path.Combine(_environment.WebRootPath, "avatar", newFileName);

                // Ensure directory exists
                string directoryPath = Path.GetDirectoryName(imageFullPath)!;
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Save the new image
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                Image image = new Image
                {
                    UserId = user.Id,
                    ImageFileName = newFileName,
                };

                _context.Images.Add(image);
                await _context.SaveChangesAsync();
            }

            // Assign role to the user
            string roleName = "";
            if (model.Role == 2)
            {
                roleName = "Organizer";
            }
            else if (model.Role == 1)
            {
                roleName = "Attendee";
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, roleName);

            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError("Role", error.Description); // Store role-related errors
                }
                return PartialView("_ValidationMessages", ModelState);
            }

            await transaction.CommitAsync();
            Response.Headers.Append("HX-Redirect", Url.Action("Login", "Auth"));
            return Content(string.Empty);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(ex.Message);
            // Define the Toastr event in the HX-Trigger header
            var toastrJson = "{\"closeModal\": true, \"clearValidations\": true, \"showToast\": { \"title\": \"Something unexpected happened!\", \"icon\": \"error\", \"timer\": 3000 }}";
            Response.Headers.Append("HX-Trigger", toastrJson);
            return Content(string.Empty);
        }
    }

    [Route("logout")]
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        Response.Headers.Append("HX-Redirect", Url.Action("Index", "Home"));
        return Content(string.Empty);
    }
}

