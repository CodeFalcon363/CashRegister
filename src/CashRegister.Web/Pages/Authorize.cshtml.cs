using CashRegister.Application.Interfaces;
using CashRegister.Domain.Entities;
using CashRegister.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace CashRegister.Web.Pages;

// Authorizer page for approving or rejecting submitted cash entries.
// Branch-isolated: authorizers only see entries from their assigned branch.
[Authorize(Policy = "AuthorizerOnly")]
public class AuthorizeModel : PageModel
{
    private readonly ICashEntryService _cashEntryService;

    public AuthorizeModel(ICashEntryService cashEntryService)
    {
        _cashEntryService = cashEntryService;
    }

    public List<CashEntry> PendingEntries { get; set; } = new();
    public CashEntry? SelectedEntry { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EntryId { get; set; }

    [BindProperty]
    public string? RejectionReason { get; set; }

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var branchId = int.Parse(User.FindFirstValue("BranchId")!);

        PendingEntries = await _cashEntryService.GetEntriesByBranchAsync(branchId, EntryStatus.Submitted);

        if (EntryId.HasValue)
        {
            SelectedEntry = await _cashEntryService.GetEntryByIdAsync(EntryId.Value);

            if (SelectedEntry == null || SelectedEntry.BranchId != branchId)
            {
                ErrorMessage = "Entry not found or access denied";
                return Page();
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int entryId)
    {
        try
        {
            var authorizerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var branchId = int.Parse(User.FindFirstValue("BranchId")!);

            var entry = await _cashEntryService.GetEntryByIdAsync(entryId);
            if (entry == null || entry.BranchId != branchId)
            {
                ErrorMessage = "Entry not found or access denied";
                return Page();
            }

            await _cashEntryService.ApproveEntryAsync(entryId, authorizerId);

            SuccessMessage = "Entry approved successfully";
            return RedirectToPage(new { });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return await OnGetAsync();
        }
    }

    public async Task<IActionResult> OnPostRejectAsync(int entryId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(RejectionReason))
            {
                ErrorMessage = "Rejection reason is required";
                EntryId = entryId;
                return await OnGetAsync();
            }

            var authorizerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var branchId = int.Parse(User.FindFirstValue("BranchId")!);

            var entry = await _cashEntryService.GetEntryByIdAsync(entryId);
            if (entry == null || entry.BranchId != branchId)
            {
                ErrorMessage = "Entry not found or access denied";
                return Page();
            }

            await _cashEntryService.RejectEntryAsync(entryId, authorizerId, RejectionReason);

            SuccessMessage = "Entry rejected successfully";
            return RedirectToPage(new { });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            EntryId = entryId;
            return await OnGetAsync();
        }
    }
}
