using CashRegister.Application.Interfaces;
using CashRegister.Domain.Entities;
using CashRegister.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace CashRegister.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class EntryDetailModel : PageModel
{
    private readonly ICashEntryService _cashEntryService;

    public EntryDetailModel(ICashEntryService cashEntryService)
    {
        _cashEntryService = cashEntryService;
    }

    public CashEntry? Entry { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Entry = await _cashEntryService.GetEntryByIdAsync(id);

        if (Entry == null)
        {
            return Page();
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

        Entry = await _cashEntryService.GetEntryByIdAsync(entryId);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int entryId)
    {
        try
        {
            await _cashEntryService.DeleteEntryAsync(entryId);
            return RedirectToPage("/Admin/Entries", new { successMessage = "Entry deleted successfully" });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting entry: {ex.Message}";
            Entry = await _cashEntryService.GetEntryByIdAsync(entryId);
            return Page();
        }
    }
}
