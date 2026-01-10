using CashRegister.Application.Interfaces;
using CashRegister.Domain.Entities;
using CashRegister.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CashRegister.Infrastructure.Services;

// Handles user authentication by verifying credentials and generating access tokens.
// Only active users can authenticate. Returns user with branch relationship loaded.
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public AuthService(
        ApplicationDbContext context,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _context = context;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<(bool Success, string? Token, User? User)> LoginAsync(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

        if (user == null)
            return (false, null, null);

        if (!_passwordService.VerifyPassword(password, user.PasswordHash))
            return (false, null, null);

        var token = _tokenService.GenerateAccessToken(user);

        return (true, token, user);
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
    }
}
