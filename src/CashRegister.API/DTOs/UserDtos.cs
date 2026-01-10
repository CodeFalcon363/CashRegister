namespace CashRegister.API.DTOs;

public record UserDto(
    int Id,
    string Username,
    string Email,
    string Role,
    int? BranchId,
    string? BranchName,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    string Role,
    int? BranchId
);

public record UpdateUserRequest(
    string Username,
    string Email,
    string Role,
    int? BranchId,
    bool IsActive
);

public record ChangePasswordRequest(
    int UserId,
    string NewPassword
);
