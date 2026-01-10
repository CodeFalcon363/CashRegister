using CashRegister.Domain.Entities;
using CashRegister.Domain.Enums;

namespace CashRegister.Application.Interfaces;

public interface ICashEntryService
{
    Task<CashEntry?> GetEntryByIdAsync(int entryId);
    Task<CashEntry?> GetEntryByBranchAndDateAsync(int branchId, DateTime date);
    Task<List<CashEntry>> GetEntriesByBranchAsync(int branchId, EntryStatus? status = null);
    Task<List<CashEntry>> GetEntriesByStatusAsync(EntryStatus status);
    Task<List<CashEntry>> GetApprovedEntriesAsync();
    Task<CashEntry> CreateDraftAsync(int branchId, DateTime entryDate, int userId);
    Task SaveRowsAsync(int entryId, List<CashEntryRow> rows);
    Task SubmitEntryAsync(int entryId);
    Task ApproveEntryAsync(int entryId, int authorizerId);
    Task RejectEntryAsync(int entryId, int authorizerId, string reason);
    Task<CashEntry?> GetPreviousDayEntryAsync(int branchId, DateTime currentDate);
    Task<List<Branch>> GetAllBranchesAsync();
    Task<List<CashEntry>> GetAllEntriesAsync();
    Task ChangeEntryStatusAsync(int entryId, EntryStatus newStatus, int userId, string? rejectionReason = null);
    Task DeleteEntryAsync(int entryId);
}
