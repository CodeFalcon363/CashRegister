using CashRegister.Domain.Entities;

namespace CashRegister.Application.Interfaces;

public interface IBranchService
{
    Task<List<Branch>> GetAllBranchesAsync();
    Task<Branch?> GetBranchByIdAsync(int id);
    Task<Branch> CreateBranchAsync(string branchCode, string branchName);
    Task UpdateBranchAsync(int id, string branchCode, string branchName);
    Task DeleteBranchAsync(int id);
}
