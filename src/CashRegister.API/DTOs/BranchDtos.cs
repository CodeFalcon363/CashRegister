namespace CashRegister.API.DTOs;

public record BranchDto(
    int Id,
    string BranchCode,
    string BranchName,
    DateTime CreatedAt
);

public record CreateBranchRequest(
    string BranchCode,
    string BranchName
);

public record UpdateBranchRequest(
    string BranchCode,
    string BranchName
);
