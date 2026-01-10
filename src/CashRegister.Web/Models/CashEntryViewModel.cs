namespace CashRegister.Web.Models;

public class CashEntryViewModel
{
    public int? EntryId { get; set; }
    public DateTime EntryDate { get; set; }
    public string Status { get; set; } = "Draft";
    public List<CashRowViewModel> Rows { get; set; } = new();

    // Rejection info
    public string? RejectionReason { get; set; }
    public UserViewModel? AuthorizedBy { get; set; }
    public DateTime? AuthorizedAt { get; set; }

    public bool WasRejected() => !string.IsNullOrEmpty(RejectionReason);
}

public class UserViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
}

public class CashRowViewModel
{
    public string RowType { get; set; } = string.Empty;
    public bool IsOutflow { get; set; }
    public int SequenceOrder { get; set; }
    public decimal Amount1000 { get; set; }
    public decimal Amount500 { get; set; }
    public decimal Amount200 { get; set; }
    public decimal Amount100 { get; set; }
    public decimal Amount50 { get; set; }
    public decimal Amount20 { get; set; }
    public decimal Amount10 { get; set; }
    public decimal Amount5 { get; set; }
    public decimal Amount2 { get; set; }
    public decimal Amount1 { get; set; }
    public decimal CoinAmount { get; set; }
}
