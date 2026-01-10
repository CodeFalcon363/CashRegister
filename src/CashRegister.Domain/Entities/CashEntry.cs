using CashRegister.Domain.Common;
using CashRegister.Domain.Enums;

namespace CashRegister.Domain.Entities;

// Daily cash register entry following maker-checker workflow: Draft → Submitted → Approved/Rejected.
// One entry per branch per day. Only one Draft or Submitted entry allowed at a time per branch.
// Rejected entries return to Draft status for correction and resubmission.
public class CashEntry : BaseEntity
{
    public int BranchId { get; private set; }
    public Branch Branch { get; private set; }
    public DateTime EntryDate { get; private set; }
    public EntryStatus Status { get; private set; }
    public int CreatedByUserId { get; private set; }
    public User CreatedBy { get; private set; }
    public int? AuthorizedByUserId { get; private set; }
    public User? AuthorizedBy { get; private set; }
    public DateTime? AuthorizedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    private readonly List<CashEntryRow> _rows = new();
    public IReadOnlyCollection<CashEntryRow> Rows => _rows.AsReadOnly();

    private CashEntry() { }

    public CashEntry(int branchId, DateTime entryDate, int createdByUserId)
    {
        if (entryDate.Date != DateTime.Today)
            throw new ArgumentException("Cash entry can only be created for today's date", nameof(entryDate));

        BranchId = branchId;
        EntryDate = entryDate.Date;
        CreatedByUserId = createdByUserId;
        Status = EntryStatus.Draft;
    }

    public void AddRow(CashEntryRow row)
    {
        if (Status != EntryStatus.Draft)
            throw new InvalidOperationException("Cannot modify entry that is not in draft status");

        _rows.Add(row);
        UpdateTimestamp();
    }

    public void Submit()
    {
        if (Status != EntryStatus.Draft)
            throw new InvalidOperationException("Only draft entries can be submitted");

        if (!_rows.Any())
            throw new InvalidOperationException("Cannot submit empty entry");

        Status = EntryStatus.Submitted;
        // Clear rejection info when resubmitting (but keep history in audit trail)
        if (WasRejected())
        {
            RejectionReason = null;
            AuthorizedByUserId = null;
            AuthorizedAt = null;
        }
        UpdateTimestamp();
    }

    public void Approve(int authorizedByUserId)
    {
        if (Status != EntryStatus.Submitted)
            throw new InvalidOperationException("Only submitted entries can be approved");

        Status = EntryStatus.Approved;
        AuthorizedByUserId = authorizedByUserId;
        AuthorizedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Reject(int rejectedByUserId, string reason)
    {
        if (Status != EntryStatus.Submitted)
            throw new InvalidOperationException("Only submitted entries can be rejected");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required", nameof(reason));

        Status = EntryStatus.Draft; // Return to draft so inputter can edit and resubmit
        AuthorizedByUserId = rejectedByUserId;
        AuthorizedAt = DateTime.UtcNow;
        RejectionReason = reason;
        UpdateTimestamp();
    }

    public bool WasRejected() => !string.IsNullOrEmpty(RejectionReason);

    public void ClearRows()
    {
        if (Status != EntryStatus.Draft)
            throw new InvalidOperationException("Cannot modify entry that is not in draft status");

        _rows.Clear();
        UpdateTimestamp();
    }

    public void ClearRejection()
    {
        RejectionReason = null;
        AuthorizedByUserId = null;
        AuthorizedAt = null;
    }

    public void AdminSetStatus(EntryStatus newStatus, int adminUserId, string? rejectionReason = null)
    {
        if (newStatus == EntryStatus.Rejected && string.IsNullOrWhiteSpace(rejectionReason))
            throw new ArgumentException("Rejection reason is required when setting status to Rejected", nameof(rejectionReason));

        Status = newStatus;

        if (newStatus == EntryStatus.Approved || newStatus == EntryStatus.Rejected)
        {
            AuthorizedByUserId = adminUserId;
            AuthorizedAt = DateTime.UtcNow;
        }

        if (newStatus == EntryStatus.Rejected)
        {
            RejectionReason = rejectionReason;
        }
        else if (newStatus == EntryStatus.Draft || newStatus == EntryStatus.Submitted)
        {
            RejectionReason = null;
            AuthorizedByUserId = null;
            AuthorizedAt = null;
        }

        UpdateTimestamp();
    }
}
