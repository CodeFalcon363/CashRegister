using CashRegister.Domain.Common;
using CashRegister.Domain.Enums;

namespace CashRegister.Domain.Entities;

// Represents a system user with role-based access control.
// Inputers and Authorizers must be assigned to a branch for data isolation.
// Viewers and Admins operate system-wide without branch assignment.
public class User : BaseEntity
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public int? BranchId { get; private set; }
    public Branch? Branch { get; private set; }
    public bool IsActive { get; private set; }

    private User() { } // EF Core

    public User(string username, string email, string passwordHash, UserRole role, int? branchId = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        if (role != UserRole.Viewer && role != UserRole.Admin && branchId == null)
            throw new ArgumentException("Inputer and Authorizer must be assigned to a branch", nameof(branchId));

        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        BranchId = branchId;
        IsActive = true;
    }

    public bool CanAccessBranch(int branchId)
    {
        if (Role == UserRole.Viewer)
            return true;

        return BranchId == branchId;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        UpdateTimestamp();
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

    public void UpdateDetails(string username, string email, UserRole role, int? branchId)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (role != UserRole.Viewer && role != UserRole.Admin && branchId == null)
            throw new ArgumentException("Inputer and Authorizer must be assigned to a branch", nameof(branchId));

        Username = username;
        Email = email;
        Role = role;
        BranchId = branchId;
        UpdateTimestamp();
    }
}
