using CashRegister.Application.Interfaces;
using CashRegister.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CashRegister.Web.Pages.Admin;

// Admin page for branch management with CRUD operations.
// Deletion protected: branches with assigned users or entries cannot be deleted.
[Authorize(Policy = "AdminOnly")]
public class BranchesModel : PageModel
{
    private readonly IBranchService _branchService;

    public BranchesModel(IBranchService branchService)
    {
        _branchService = branchService;
    }

    public List<Branch> Branches { get; set; } = new();
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Branches = await _branchService.GetAllBranchesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(string branchCode, string branchName)
    {
        try
        {
            await _branchService.CreateBranchAsync(branchCode, branchName);
            SuccessMessage = "Branch created successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error creating branch: {ex.Message}";
        }

        Branches = await _branchService.GetAllBranchesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int branchId, string branchCode, string branchName)
    {
        try
        {
            await _branchService.UpdateBranchAsync(branchId, branchCode, branchName);
            SuccessMessage = "Branch updated successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error updating branch: {ex.Message}";
        }

        Branches = await _branchService.GetAllBranchesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int branchId)
    {
        try
        {
            await _branchService.DeleteBranchAsync(branchId);
            SuccessMessage = "Branch deleted successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting branch: {ex.Message}";
        }

        Branches = await _branchService.GetAllBranchesAsync();
        return Page();
    }
}
