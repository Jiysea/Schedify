using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Schedify.Data;
using Schedify.Models;

namespace Schedify.Services;

public class UserService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<User> _userManager;

    public UserService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
    {
        _context = context;
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

    public string? GetUserAvatarFileName()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userId = Guid.Parse(GetUserId()!);
        return _context.Images.Where(i => i.UserId == userId).Select(i => i.ImageFileName).FirstOrDefault();
    }

    public async Task<string?> GetUserAvatarFileNameAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var userId = Guid.Parse(GetUserId()!);
        return await _context.Images.Where(i => i.UserId == userId).Select(i => i.ImageFileName).FirstOrDefaultAsync();
    }
}
