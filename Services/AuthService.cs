using BCrypt.Net;
using Schedify.Models;
using Schedify.Repositories;

namespace Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> RegisterUserAsync(string firstName, string? middleName, string lastName, string? extensionName, DateTime birthdate, string phoneNum, UserRoles role, string email, string password)
    {
        if (await _userRepository.GetUserByEmailAsync(email) != null)
        {
            return false;
        }

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            ExtensionName = extensionName,
            Birthdate = birthdate,
            PhoneNum = phoneNum,
            Role = role,
            Email = email,
            PasswordHash = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateUserAsync(user);
        return true;
    }

    public async Task<User?> ValidateUserAsync(string email, string password)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null;
        }

        return user;
    }
}