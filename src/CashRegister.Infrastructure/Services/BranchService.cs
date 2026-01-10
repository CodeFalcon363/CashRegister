using CashRegister.Application.Interfaces;
using CashRegister.Domain.Entities;
using CashRegister.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CashRegister.Infrastructure.Services;

// Manages branch CRUD operations with referential integrity checks.
// Prevents deletion of branches with assigned users or cash entries.
public class BranchService : IBranchService
{
    private readonly ApplicationDbContext _context;

    public BranchService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Branch>> GetAllBranchesAsync()
    {
        return await _context.Branches
            .OrderBy(b => b.BranchCode)
            .ToListAsync();
    }

    public async Task<Branch?> GetBranchByIdAsync(int id)
    {
        return await _context.Branches
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Branch> CreateBranchAsync(string branchCode, string branchName)
    {
        var branch = new Branch(branchCode, branchName);

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();

        return branch;
    }

    public async Task UpdateBranchAsync(int id, string branchCode, string branchName)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null)
        {
            throw new InvalidOperationException("Branch not found");
        }

        branch.UpdateDetails(branchCode, branchName);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteBranchAsync(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch == null)
        {
            throw new InvalidOperationException("Branch not found");
        }

        // Check if branch has users
        var hasUsers = await _context.Users.AnyAsync(u => u.BranchId == id);
        if (hasUsers)
        {
            throw new InvalidOperationException("Cannot delete branch with assigned users. Please reassign or delete users first.");
        }

        // Check if branch has entries
        var hasEntries = await _context.CashEntries.AnyAsync(e => e.BranchId == id);
        if (hasEntries)
        {
            throw new InvalidOperationException("Cannot delete branch with cash entries. Please delete entries first or deactivate the branch instead.");
        }

        _context.Branches.Remove(branch);
        await _context.SaveChangesAsync();
    }
}
