using CashRegister.Application.Interfaces;
using CashRegister.Domain.Entities;
using CashRegister.Domain.Enums;
using CashRegister.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CashRegister.Infrastructure.Services;

// Manages user accounts with CRUD operations and password management.
// Passwords are hashed using IPasswordService before storage.
public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;

    public UserService(ApplicationDbContext context, IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Branch)
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> CreateUserAsync(string username, string email, string password, UserRole role, int? branchId)
    {
        var passwordHash = _passwordService.HashPassword(password);
        var user = new User(username, email, passwordHash, role, branchId);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task UpdateUserAsync(int id, string username, string email, UserRole role, int? branchId, bool isActive)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.UpdateDetails(username, email, role, branchId);

        if (isActive && !user.IsActive)
        {
            user.Activate();
        }
        else if (!isActive && user.IsActive)
        {
            user.Deactivate();
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ChangePasswordAsync(int id, string newPassword)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        var passwordHash = _passwordService.HashPassword(newPassword);
        user.UpdatePassword(passwordHash);
        await _context.SaveChangesAsync();

        return true;
    }
}
