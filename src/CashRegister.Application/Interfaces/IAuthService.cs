using CashRegister.Domain.Entities;

namespace CashRegister.Application.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string? Token, User? User)> LoginAsync(string username, string password);
    Task<User?> GetUserByIdAsync(int userId);
}
