using CashRegister.Domain.Common;

namespace CashRegister.Domain.Entities;

// Represents a bank branch location. Branch code must be exactly 3 characters.
// Protected from deletion if it has assigned users or cash entries.
public class Branch : BaseEntity
{
    public string BranchCode { get; private set; }
    public string BranchName { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private readonly List<CashEntry> _cashEntries = new();
    public IReadOnlyCollection<CashEntry> CashEntries => _cashEntries.AsReadOnly();

    private Branch() { }

    public Branch(string branchCode, string branchName)
    {
        if (string.IsNullOrWhiteSpace(branchCode) || branchCode.Length != 3)
            throw new ArgumentException("Branch code must be exactly 3 characters", nameof(branchCode));

        if (string.IsNullOrWhiteSpace(branchName))
            throw new ArgumentException("Branch name cannot be empty", nameof(branchName));

        BranchCode = branchCode;
        BranchName = branchName;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    public void UpdateDetails(string branchCode, string branchName)
    {
        if (string.IsNullOrWhiteSpace(branchCode) || branchCode.Length != 3)
            throw new ArgumentException("Branch code must be exactly 3 characters", nameof(branchCode));

        if (string.IsNullOrWhiteSpace(branchName))
            throw new ArgumentException("Branch name cannot be empty", nameof(branchName));

        BranchCode = branchCode;
        BranchName = branchName;
        UpdateTimestamp();
    }
}
