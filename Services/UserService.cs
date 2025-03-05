using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Schedify.Models;

namespace Schedify.Services;

public class UserService
{

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<User> _userManager;

    public UserService(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }
    public string? GetUserId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public string? GetUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
    }

    public async Task<User?> GetUserAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user != null ? await _userManager.GetUserAsync(user) : null;
    }
}
