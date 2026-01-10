using CashRegister.Application.Interfaces;
using CashRegister.Domain.Entities;
using CashRegister.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace CashRegister.Web.Pages;

// Read-only view of approved cash entries with filtering by branch and date range.
// Viewers see all branches; Authorizers and Inputers see only their branch.
[Authorize(Roles = "Viewer,Authorizer,Inputer")]
public class ViewModel : PageModel
{
    private readonly ICashEntryService _cashEntryService;

    public ViewModel(ICashEntryService cashEntryService)
    {
        _cashEntryService = cashEntryService;
    }

    public List<CashEntry> ApprovedEntries { get; set; } = new();
    public CashEntry? SelectedEntry { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EntryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? BranchFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? DateFrom { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? DateTo { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        ApprovedEntries = await _cashEntryService.GetApprovedEntriesAsync();

        // Filter by branch for Authorizers and Inputers (only show their branch)
        // Viewers can see all branches
        if (User.IsInRole("Authorizer") || User.IsInRole("Inputer"))
        {
            var branchId = int.Parse(User.FindFirstValue("BranchId")!);
            ApprovedEntries = ApprovedEntries.Where(e => e.BranchId == branchId).ToList();
        }

        if (!string.IsNullOrEmpty(BranchFilter))
        {
            ApprovedEntries = ApprovedEntries.Where(e => e.Branch.BranchCode == BranchFilter).ToList();
        }

        if (DateFrom.HasValue)
        {
            ApprovedEntries = ApprovedEntries.Where(e => e.EntryDate >= DateFrom.Value).ToList();
        }

        if (DateTo.HasValue)
        {
            ApprovedEntries = ApprovedEntries.Where(e => e.EntryDate <= DateTo.Value).ToList();
        }

        if (EntryId.HasValue)
        {
            SelectedEntry = await _cashEntryService.GetEntryByIdAsync(EntryId.Value);

            if (SelectedEntry == null || SelectedEntry.Status != EntryStatus.Approved)
            {
                ErrorMessage = "Entry not found or not approved";
                return Page();
            }

            // Verify branch access for Authorizers and Inputers
            if ((User.IsInRole("Authorizer") || User.IsInRole("Inputer")) && SelectedEntry.BranchId != int.Parse(User.FindFirstValue("BranchId")!))
            {
                ErrorMessage = "Access denied - entry belongs to a different branch";
                SelectedEntry = null;
                return Page();
            }
        }

        return Page();
    }
}
