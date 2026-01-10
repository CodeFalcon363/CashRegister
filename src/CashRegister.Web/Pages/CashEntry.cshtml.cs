using CashRegister.Application.Interfaces;
using CashRegister.Domain.Entities;
using CashRegister.Domain.Enums;
using CashRegister.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace CashRegister.Web.Pages;

// Inputer page for creating and editing daily cash entries with denomination tracking.
// Auto-populates opening balance from previous day's vault closing balance.
// Supports draft saving and submission for authorization.
[Authorize(Policy = "InputerOnly")]
public class CashEntryModel : PageModel
{
    private readonly ICashEntryService _cashEntryService;

    public CashEntryModel(ICashEntryService cashEntryService)
    {
        _cashEntryService = cashEntryService;
    }

    [BindProperty]
    public CashEntryViewModel Entry { get; set; } = new();

    public List<CashEntryViewModel>? AllEntries { get; set; }
    public List<CashEntryViewModel>? FilteredEntries { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EntryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Action { get; set; }

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var branchId = int.Parse(User.FindFirstValue("BranchId")!);
        var today = DateTime.Today;

        // Load all entries for sidebar counts
        var allBranchEntries = await _cashEntryService.GetEntriesByBranchAsync(branchId);
        AllEntries = allBranchEntries.Select(MapToViewModel).ToList();

        // Handle filtering for viewing submitted/authorized entries
        if (!string.IsNullOrEmpty(StatusFilter))
        {
            FilteredEntries = AllEntries.Where(e => e.Status == StatusFilter).OrderByDescending(e => e.EntryDate).ToList();

            // If entryId is specified, load that entry
            if (EntryId.HasValue)
            {
                var selectedEntry = await _cashEntryService.GetEntryByIdAsync(EntryId.Value);
                if (selectedEntry != null && selectedEntry.BranchId == branchId)
                {
                    Entry = MapToViewModel(selectedEntry);
                }
                else
                {
                    ErrorMessage = "Entry not found or access denied";
                }
            }
            // Don't auto-load the first entry - let user select from list
        }
        else if (Action == "create")
        {
            // Load or create entry for editing
            if (EntryId.HasValue)
            {
                // Load specific entry by ID (for editing drafts from list)
                var selectedEntry = await _cashEntryService.GetEntryByIdAsync(EntryId.Value);
                if (selectedEntry != null && selectedEntry.BranchId == branchId)
                {
                    Entry = MapToViewModel(selectedEntry);
                }
                else
                {
                    ErrorMessage = "Entry not found or access denied";
                }
            }
            else
            {
                // Load or create today's entry
                var existingEntry = await _cashEntryService.GetEntryByBranchAndDateAsync(branchId, today);

                if (existingEntry != null)
                {
                    Entry = MapToViewModel(existingEntry);
                }
                else
                {
                    Entry.EntryDate = today;
                    Entry.Rows = InitializeRows();

                    var previousEntry = await _cashEntryService.GetPreviousDayEntryAsync(branchId, today);
                    if (previousEntry != null)
                    {
                        PopulateFromPreviousDay(previousEntry);
                    }
                }
            }
        }
        // Otherwise, show empty state (Entry will be null/empty)

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync([FromBody] CashEntryViewModel model)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var branchId = int.Parse(User.FindFirstValue("BranchId")!);

            CashEntry entry;
            if (model.EntryId.HasValue)
            {
                entry = await _cashEntryService.GetEntryByIdAsync(model.EntryId.Value)
                    ?? throw new InvalidOperationException("Entry not found");
            }
            else
            {
                entry = await _cashEntryService.CreateDraftAsync(branchId, model.EntryDate, userId);
            }

            var rows = model.Rows.Select(r => new CashEntryRow(entry.Id, r.RowType, r.IsOutflow, r.SequenceOrder)).ToList();
            for (int i = 0; i < rows.Count; i++)
            {
                var modelRow = model.Rows[i];
                rows[i].SetAmounts(
                    modelRow.Amount1000, modelRow.Amount500, modelRow.Amount200,
                    modelRow.Amount100, modelRow.Amount50, modelRow.Amount20,
                    modelRow.Amount10, modelRow.Amount5, modelRow.Amount2,
                    modelRow.Amount1, modelRow.CoinAmount);
            }

            await _cashEntryService.SaveRowsAsync(entry.Id, rows);

            return new JsonResult(new { success = true, entryId = entry.Id });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostSubmitAsync([FromBody] CashEntryViewModel model)
    {
        try
        {
            if (!model.EntryId.HasValue)
            {
                return new JsonResult(new { success = false, error = "Please save the entry first" });
            }

            await _cashEntryService.SubmitEntryAsync(model.EntryId.Value);

            return new JsonResult(new { success = true, message = "Entry submitted for authorization" });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }

    private List<CashRowViewModel> InitializeRows()
    {
        var rowDefinitions = new[]
        {
            ("Opening Balance", false, 1),
            ("Till Out", true, 2),
            ("Vault Out Bulk", true, 3),
            ("Vault Out Teller 1", true, 4),
            ("Vault Out Teller 2", true, 5),
            ("Vault Out Teller 3", true, 6),
            ("Vault Out Teller 4", true, 7),
            ("Vault Out Teller 5", true, 8),
            ("Vault Out Teller 6", true, 9),
            ("Load ATM 1", true, 10),
            ("Load ATM 2", true, 11),
            ("Load ATM 3", true, 12),
            ("Load ATM 4", true, 13),
            ("Load ATM 5", true, 14),
            ("Load ATM 6", true, 15),
            ("Load ATM 7", true, 16),
            ("Load ATM 8", true, 17),
            ("Unload ATM 1", false, 18),
            ("Unload ATM 2", false, 19),
            ("Unload ATM 3", false, 20),
            ("Unload ATM 4", false, 21),
            ("Unload ATM 5", false, 22),
            ("Unload ATM 6", false, 23),
            ("Unload ATM 7", false, 24),
            ("Unload ATM 8", false, 25),
            ("BSU SUPPLY", false, 26),
            ("BSU EVACUATION", true, 27),
            ("Vault In Teller 1", false, 28),
            ("Vault In Teller 2", false, 29),
            ("Vault In Teller 3", false, 30),
            ("Vault In Teller 4", false, 31),
            ("Vault In Teller 5", false, 32),
            ("Vault In Teller 6", false, 33),
            ("Vault In Bulk", false, 34),
            ("Vault Figure", false, 35),
            ("Till Total", false, 36),
            ("Vault Closing Balance", false, 37)
        };

        return rowDefinitions.Select(r => new CashRowViewModel
        {
            RowType = r.Item1,
            IsOutflow = r.Item2,
            SequenceOrder = r.Item3
        }).ToList();
    }

    private void PopulateFromPreviousDay(CashEntry previousEntry)
    {
        var vaultClosingRow = previousEntry.Rows.FirstOrDefault(r => r.RowType == "Vault Closing Balance");
        var tillTotalRow = previousEntry.Rows.FirstOrDefault(r => r.RowType == "Till Total");

        if (vaultClosingRow != null)
        {
            var openingBalanceRow = Entry.Rows.First(r => r.RowType == "Opening Balance");
            openingBalanceRow.Amount1000 = vaultClosingRow.Amount1000;
            openingBalanceRow.Amount500 = vaultClosingRow.Amount500;
            openingBalanceRow.Amount200 = vaultClosingRow.Amount200;
            openingBalanceRow.Amount100 = vaultClosingRow.Amount100;
            openingBalanceRow.Amount50 = vaultClosingRow.Amount50;
            openingBalanceRow.Amount20 = vaultClosingRow.Amount20;
            openingBalanceRow.Amount10 = vaultClosingRow.Amount10;
            openingBalanceRow.Amount5 = vaultClosingRow.Amount5;
            openingBalanceRow.Amount2 = vaultClosingRow.Amount2;
            openingBalanceRow.Amount1 = vaultClosingRow.Amount1;
            openingBalanceRow.CoinAmount = vaultClosingRow.CoinAmount;
        }

        if (tillTotalRow != null)
        {
            var tillOutRow = Entry.Rows.First(r => r.RowType == "Till Out");
            tillOutRow.Amount1000 = tillTotalRow.Amount1000;
            tillOutRow.Amount500 = tillTotalRow.Amount500;
            tillOutRow.Amount200 = tillTotalRow.Amount200;
            tillOutRow.Amount100 = tillTotalRow.Amount100;
            tillOutRow.Amount50 = tillTotalRow.Amount50;
            tillOutRow.Amount20 = tillTotalRow.Amount20;
            tillOutRow.Amount10 = tillTotalRow.Amount10;
            tillOutRow.Amount5 = tillTotalRow.Amount5;
            tillOutRow.Amount2 = tillTotalRow.Amount2;
            tillOutRow.Amount1 = tillTotalRow.Amount1;
            tillOutRow.CoinAmount = tillTotalRow.CoinAmount;
        }
    }

    private CashEntryViewModel MapToViewModel(CashEntry entry)
    {
        return new CashEntryViewModel
        {
            EntryId = entry.Id,
            EntryDate = entry.EntryDate,
            Status = entry.Status.ToString(),
            RejectionReason = entry.RejectionReason,
            AuthorizedBy = entry.AuthorizedBy != null ? new UserViewModel
            {
                Id = entry.AuthorizedBy.Id,
                Username = entry.AuthorizedBy.Username
            } : null,
            AuthorizedAt = entry.AuthorizedAt,
            Rows = entry.Rows.Select(r => new CashRowViewModel
            {
                RowType = r.RowType,
                IsOutflow = r.IsOutflow,
                SequenceOrder = r.SequenceOrder,
                Amount1000 = r.Amount1000,
                Amount500 = r.Amount500,
                Amount200 = r.Amount200,
                Amount100 = r.Amount100,
                Amount50 = r.Amount50,
                Amount20 = r.Amount20,
                Amount10 = r.Amount10,
                Amount5 = r.Amount5,
                Amount2 = r.Amount2,
                Amount1 = r.Amount1,
                CoinAmount = r.CoinAmount
            }).OrderBy(r => r.SequenceOrder).ToList()
        };
    }
}
