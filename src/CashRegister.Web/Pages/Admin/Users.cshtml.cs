using CashRegister.Application.Interfaces;
using CashRegister.Domain.Entities;
using CashRegister.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CashRegister.Web.Pages.Admin;

// Admin page for full CRUD operations on system users.
// Supports role assignment, branch assignment, and password management.
[Authorize(Policy = "AdminOnly")]
public class UsersModel : PageModel
{
    private readonly IUserService _userService;
    private readonly ICashEntryService _cashEntryService;

    public UsersModel(IUserService userService, ICashEntryService cashEntryService)
    {
        _userService = userService;
        _cashEntryService = cashEntryService;
    }

    public List<User> Users { get; set; } = new();
    public List<Branch> Branches { get; set; } = new();
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Users = await _userService.GetAllUsersAsync();
        Branches = await _cashEntryService.GetAllBranchesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(string username, string email, string password, UserRole role, int? branchId)
    {
        try
        {
            if (string.IsNullOrEmpty(branchId?.ToString()))
            {
                branchId = null;
            }

            await _userService.CreateUserAsync(username, email, password, role, branchId);
            SuccessMessage = "User created successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error creating user: {ex.Message}";
        }

        Users = await _userService.GetAllUsersAsync();
        Branches = await _cashEntryService.GetAllBranchesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int userId, string username, string email, UserRole role, int? branchId, bool isActive)
    {
        try
        {
            if (string.IsNullOrEmpty(branchId?.ToString()))
            {
                branchId = null;
            }

            await _userService.UpdateUserAsync(userId, username, email, role, branchId, isActive);
            SuccessMessage = "User updated successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error updating user: {ex.Message}";
        }

        Users = await _userService.GetAllUsersAsync();
        Branches = await _cashEntryService.GetAllBranchesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int userId)
    {
        try
        {
            await _userService.DeleteUserAsync(userId);
            SuccessMessage = "User deleted successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting user: {ex.Message}";
        }

        Users = await _userService.GetAllUsersAsync();
        Branches = await _cashEntryService.GetAllBranchesAsync();
        return Page();
    }
}
