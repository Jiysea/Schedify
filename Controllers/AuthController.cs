using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Schedify.Models;
using Schedify.ViewModels;

namespace Schedify.Controllers;

public class AuthController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public AuthController(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [Route("login")]
    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }

    [Route("auth/login")]
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("~/Views/Auth/Partials/_ValidationMessages.cshtml", ModelState);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            ModelState.AddModelError("Email", "User not found.");
            return PartialView("~/Views/Auth/Partials/_ValidationMessages.cshtml", ModelState);
        }
        var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return PartialView("~/Views/Auth/Partials/_ValidationMessages.cshtml", ModelState);
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Contains("Admin"))
            Response.Headers.Append("HX-Redirect", Url.Action("Dashboard", "Admin"));
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
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("~/Views/Auth/Partials/_ValidationMessages.cshtml", ModelState);
        }

        User user = new User
        {
            FirstName = model.FirstName,
            MiddleName = model.MiddleName,
            LastName = model.LastName,
            ExtensionName = model.ExtensionName,
            Birthdate = model.Birthdate,
            Email = model.Email,
            UserName = model.Email,
            PhoneNumber = model.PhoneNumber,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("Password", error.Description); // Store password-related errors
            }
            return PartialView("~/Views/Auth/Partials/_ValidationMessages.cshtml", ModelState);
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
            return PartialView("~/Views/Auth/Partials/_ValidationMessages.cshtml", ModelState);
        }

        Response.Headers.Append("HX-Redirect", Url.Action("Login", "Auth"));
        return Content(string.Empty);
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

