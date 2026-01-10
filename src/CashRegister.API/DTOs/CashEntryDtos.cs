namespace CashRegister.API.DTOs;

public record CashEntryDto(
    int Id,
    int BranchId,
    string BranchName,
    DateTime EntryDate,
    string Status,
    int CreatedByUserId,
    string CreatedByUsername,
    DateTime CreatedAt,
    int? AuthorizedByUserId,
    string? AuthorizedByUsername,
    DateTime? AuthorizedAt,
    string? RejectionReason,
    List<CashEntryRowDto> Rows
);

public record CashEntryRowDto(
    int Id,
    int SequenceOrder,
    string RowType,
    bool IsOutflow,
    decimal Amount1000,
    decimal Amount500,
    decimal Amount200,
    decimal Amount100,
    decimal Amount50,
    decimal Amount20,
    decimal Amount10,
    decimal Amount5,
    decimal Amount2,
    decimal Amount1,
    decimal AmountCoin,
    decimal Total
);

public record CreateCashEntryRequest(
    DateTime EntryDate,
    List<CreateCashEntryRowRequest> Rows
);

public record CreateCashEntryRowRequest(
    int SequenceOrder,
    string RowType,
    bool IsOutflow,
    decimal Amount1000,
    decimal Amount500,
    decimal Amount200,
    decimal Amount100,
    decimal Amount50,
    decimal Amount20,
    decimal Amount10,
    decimal Amount5,
    decimal Amount2,
    decimal Amount1,
    decimal AmountCoin
);

public record UpdateCashEntryRowRequest(
    int? Id,
    int SequenceOrder,
    string RowType,
    bool IsOutflow,
    decimal Amount1000,
    decimal Amount500,
    decimal Amount200,
    decimal Amount100,
    decimal Amount50,
    decimal Amount20,
    decimal Amount10,
    decimal Amount5,
    decimal Amount2,
    decimal Amount1,
    decimal AmountCoin
);

public record SubmitCashEntryRequest(int EntryId);

public record ApproveCashEntryRequest(int EntryId);

public record RejectCashEntryRequest(int EntryId, string RejectionReason);

public record ChangeEntryStatusRequest(int EntryId, string NewStatus, string? RejectionReason);
