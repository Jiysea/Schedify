using Microsoft.AspNetCore.Mvc;
using Services;

namespace Schedify.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [Route("login")]
    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _authService.ValidateUserAsync(email, password);
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View();
        }

        // Store user session
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserFullName", $"{user.FirstName} {user.LastName}");

        return RedirectToAction("Dashboard", "Organizer");
    }

    [Route("register")]
    [HttpGet]
    public ActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(string firstName, string? middleName, string lastName, string? extensionName, DateTime birthdate, string phoneNum, UserRoles role, string email, string password)
    {
        bool isRegistered = await _authService.RegisterUserAsync(firstName, middleName, lastName, extensionName, birthdate, phoneNum, role, email, password);
        if (!isRegistered)
        {
            ModelState.AddModelError("", "Email already in use.");
            return View();
        }

        return RedirectToAction("Login");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); // Remove session data
        return RedirectToAction("Login");
    }
}

