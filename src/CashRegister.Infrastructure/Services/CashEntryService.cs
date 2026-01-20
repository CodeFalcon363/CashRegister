using CashRegister.Application.Interfaces;
using CashRegister.Domain.Entities;
using CashRegister.Domain.Enums;
using CashRegister.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CashRegister.Infrastructure.Services;

// Manages cash entry lifecycle and workflow transitions.
// Handles draft creation, row management, submission, approval/rejection, and admin overrides.
// Enforces one entry per branch per day constraint.
public class CashEntryService : ICashEntryService
{
    private readonly ApplicationDbContext _context;

    public CashEntryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CashEntry?> GetEntryByIdAsync(int entryId)
    {
        return await _context.CashEntries
            .Include(ce => ce.Branch)
            .Include(ce => ce.CreatedBy)
            .Include(ce => ce.AuthorizedBy)
            .Include(ce => ce.Rows.OrderBy(r => r.SequenceOrder))
            .FirstOrDefaultAsync(ce => ce.Id == entryId);
    }

    public async Task<CashEntry?> GetEntryByBranchAndDateAsync(int branchId, DateTime date)
    {
        return await _context.CashEntries
            .Include(ce => ce.Branch)
            .Include(ce => ce.CreatedBy)
            .Include(ce => ce.AuthorizedBy)
            .Include(ce => ce.Rows.OrderBy(r => r.SequenceOrder))
            .FirstOrDefaultAsync(ce => ce.BranchId == branchId && ce.EntryDate.Date == date.Date);
    }

    public async Task<List<CashEntry>> GetEntriesByBranchAsync(int branchId, EntryStatus? status = null)
    {
        var query = _context.CashEntries
            .Include(ce => ce.CreatedBy)
            .Include(ce => ce.AuthorizedBy)
            .Include(ce => ce.Rows)
            .Where(ce => ce.BranchId == branchId);

        if (status.HasValue)
        {
            query = query.Where(ce => ce.Status == status.Value);
        }

        return await query
            .OrderByDescending(ce => ce.EntryDate)
            .ToListAsync();
    }

    public async Task<List<CashEntry>> GetEntriesByStatusAsync(EntryStatus status)
    {
        return await _context.CashEntries
            .Include(ce => ce.Branch)
            .Include(ce => ce.CreatedBy)
            .Where(ce => ce.Status == status)
            .OrderByDescending(ce => ce.EntryDate)
            .ToListAsync();
    }

    public async Task<List<CashEntry>> GetApprovedEntriesAsync()
    {
        return await _context.CashEntries
            .Include(ce => ce.Branch)
            .Include(ce => ce.CreatedBy)
            .Include(ce => ce.AuthorizedBy)
            .Where(ce => ce.Status == EntryStatus.Approved)
            .OrderByDescending(ce => ce.EntryDate)
            .ToListAsync();
    }

    public async Task<CashEntry> CreateDraftAsync(int branchId, DateTime entryDate, int userId)
    {
        var existingEntry = await GetEntryByBranchAndDateAsync(branchId, entryDate);
        if (existingEntry != null)
        {
            throw new InvalidOperationException("An entry already exists for this branch and date");
        }

        var entry = new CashEntry(branchId, entryDate, userId);
        _context.CashEntries.Add(entry);
        await _context.SaveChangesAsync();

        return entry;
    }

    public async Task SaveRowsAsync(int entryId, List<CashEntryRow> rows)
    {
        var entry = await GetEntryByIdAsync(entryId);
        if (entry == null)
        {
            throw new InvalidOperationException("Entry not found");
        }

        if (entry.Status != EntryStatus.Draft)
        {
            throw new InvalidOperationException("Can only modify draft entries");
        }

        var existingRows = _context.CashEntryRows.Where(r => r.CashEntryId == entryId);
        _context.CashEntryRows.RemoveRange(existingRows);

        foreach (var row in rows)
        {
            entry.AddRow(row);
        }

        await _context.SaveChangesAsync();
    }

    public async Task SubmitEntryAsync(int entryId)
    {
        var entry = await GetEntryByIdAsync(entryId);
        if (entry == null)
        {
            throw new InvalidOperationException("Entry not found");
        }

        entry.Submit();
        await _context.SaveChangesAsync();
    }

    public async Task ApproveEntryAsync(int entryId, int authorizerId)
    {
        var entry = await GetEntryByIdAsync(entryId);
        if (entry == null)
        {
            throw new InvalidOperationException("Entry not found");
        }

        entry.Approve(authorizerId);
        await _context.SaveChangesAsync();
    }

    public async Task RejectEntryAsync(int entryId, int authorizerId, string reason)
    {
        var entry = await GetEntryByIdAsync(entryId);
        if (entry == null)
        {
            throw new InvalidOperationException("Entry not found");
        }

        entry.Reject(authorizerId, reason);
        await _context.SaveChangesAsync();
    }

    public async Task<CashEntry?> GetPreviousDayEntryAsync(int branchId, DateTime currentDate)
    {
        var previousDate = currentDate.AddDays(-1);
        return await _context.CashEntries
            .Include(ce => ce.Branch)
            .Include(ce => ce.CreatedBy)
            .Include(ce => ce.AuthorizedBy)
            .Include(ce => ce.Rows.OrderBy(r => r.SequenceOrder))
            .Where(ce => ce.BranchId == branchId
                && ce.EntryDate.Date == previousDate.Date
                && ce.Status == EntryStatus.Approved)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Branch>> GetAllBranchesAsync()
    {
        return await _context.Branches
            .OrderBy(b => b.BranchCode)
            .ToListAsync();
    }

    public async Task<List<CashEntry>> GetAllEntriesAsync()
    {
        return await _context.CashEntries
            .Include(ce => ce.Branch)
            .Include(ce => ce.CreatedBy)
            .Include(ce => ce.AuthorizedBy)
            .Include(ce => ce.Rows)
            .OrderByDescending(ce => ce.EntryDate)
            .ToListAsync();
    }

    public async Task ChangeEntryStatusAsync(int entryId, EntryStatus newStatus, int userId, string? rejectionReason = null)
    {
        var entry = await GetEntryByIdAsync(entryId);
        if (entry == null)
        {
            throw new InvalidOperationException("Entry not found");
        }

        entry.AdminSetStatus(newStatus, userId, rejectionReason);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteEntryAsync(int entryId)
    {
        var entry = await _context.CashEntries
            .Include(ce => ce.Rows)
            .FirstOrDefaultAsync(ce => ce.Id == entryId);

        if (entry == null)
        {
            throw new InvalidOperationException("Entry not found");
        }

        _context.CashEntries.Remove(entry);
        await _context.SaveChangesAsync();
    }
}
