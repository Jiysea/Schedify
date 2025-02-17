using Schedify.Models;

namespace Services;

public interface IAuthService
{
    Task<bool> RegisterUserAsync(string firstName, string? middleName, string lastName, string? extensionName, DateTime birthdate, string phoneNum, UserRoles role, string email, string password);
    Task<User?> ValidateUserAsync(string email, string password);
}