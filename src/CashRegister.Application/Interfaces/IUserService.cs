using CashRegister.Domain.Entities;
using CashRegister.Domain.Enums;

namespace CashRegister.Application.Interfaces;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User> CreateUserAsync(string username, string email, string password, UserRole role, int? branchId);
    Task UpdateUserAsync(int id, string username, string email, UserRole role, int? branchId, bool isActive);
    Task DeleteUserAsync(int id);
    Task<bool> ChangePasswordAsync(int id, string newPassword);
}
