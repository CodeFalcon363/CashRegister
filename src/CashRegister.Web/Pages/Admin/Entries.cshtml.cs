using CashRegister.Application.Interfaces;
using CashRegister.Domain.Entities;
using CashRegister.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace CashRegister.Web.Pages.Admin;

// Admin page for entry oversight with status override and deletion capabilities.
// Supports filtering by branch and status for bulk operations.
[Authorize(Policy = "AdminOnly")]
public class EntriesModel : PageModel
{
    private readonly ICashEntryService _cashEntryService;

    public EntriesModel(ICashEntryService cashEntryService)
    {
        _cashEntryService = cashEntryService;
    }

    public List<CashEntry> Entries { get; set; } = new();
    public List<Branch> Branches { get; set; } = new();
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? BranchIdFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Branches = await _cashEntryService.GetAllBranchesAsync();
        Entries = await _cashEntryService.GetAllEntriesAsync();

        if (BranchIdFilter.HasValue)
        {
            Entries = Entries.Where(e => e.BranchId == BranchIdFilter.Value).ToList();
        }

        if (!string.IsNullOrEmpty(StatusFilter))
        {
            if (Enum.TryParse<EntryStatus>(StatusFilter, out var status))
            {
                Entries = Entries.Where(e => e.Status == status).ToList();
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostChangeStatusAsync(int entryId, EntryStatus newStatus, string? rejectionReason)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _cashEntryService.ChangeEntryStatusAsync(entryId, newStatus, userId, rejectionReason);
            SuccessMessage = $"Entry status changed to {newStatus}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error changing status: {ex.Message}";
        }

        Branches = await _cashEntryService.GetAllBranchesAsync();
        Entries = await _cashEntryService.GetAllEntriesAsync();

        if (BranchIdFilter.HasValue)
        {
            Entries = Entries.Where(e => e.BranchId == BranchIdFilter.Value).ToList();
        }

        if (!string.IsNullOrEmpty(StatusFilter))
        {
            if (Enum.TryParse<EntryStatus>(StatusFilter, out var status))
            {
                Entries = Entries.Where(e => e.Status == status).ToList();
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int entryId)
    {
        try
        {
            await _cashEntryService.DeleteEntryAsync(entryId);
            SuccessMessage = "Entry deleted successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting entry: {ex.Message}";
        }

        Branches = await _cashEntryService.GetAllBranchesAsync();
        Entries = await _cashEntryService.GetAllEntriesAsync();

        if (BranchIdFilter.HasValue)
        {
            Entries = Entries.Where(e => e.BranchId == BranchIdFilter.Value).ToList();
        }

        if (!string.IsNullOrEmpty(StatusFilter))
        {
            if (Enum.TryParse<EntryStatus>(StatusFilter, out var status))
            {
                Entries = Entries.Where(e => e.Status == status).ToList();
            }
        }

        return Page();
    }
}
