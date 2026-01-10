using CashRegister.Domain.Common;

namespace CashRegister.Domain.Entities;

// Individual row within a cash entry representing a transaction type with denomination breakdowns.
// Tracks amounts for each currency denomination (1000, 500, 200, 100, 50, 20, 10, 5, 2, 1, and coins).
// IsOutflow marks negative amounts (payments out, transfers out).
public class CashEntryRow : BaseEntity
{
    public int CashEntryId { get; private set; }
    public CashEntry CashEntry { get; private set; }
    public string RowType { get; private set; }
    public bool IsOutflow { get; private set; }
    public int SequenceOrder { get; private set; }

    public decimal Amount1000 { get; private set; }
    public decimal Amount500 { get; private set; }
    public decimal Amount200 { get; private set; }
    public decimal Amount100 { get; private set; }
    public decimal Amount50 { get; private set; }
    public decimal Amount20 { get; private set; }
    public decimal Amount10 { get; private set; }
    public decimal Amount5 { get; private set; }
    public decimal Amount2 { get; private set; }
    public decimal Amount1 { get; private set; }
    public decimal CoinAmount { get; private set; }

    private CashEntryRow() { }

    public CashEntryRow(int cashEntryId, string rowType, bool isOutflow, int sequenceOrder)
    {
        if (string.IsNullOrWhiteSpace(rowType))
            throw new ArgumentException("Row type cannot be empty", nameof(rowType));

        CashEntryId = cashEntryId;
        RowType = rowType;
        IsOutflow = isOutflow;
        SequenceOrder = sequenceOrder;
    }

    public void SetAmounts(
        decimal amount1000 = 0, decimal amount500 = 0, decimal amount200 = 0,
        decimal amount100 = 0, decimal amount50 = 0, decimal amount20 = 0,
        decimal amount10 = 0, decimal amount5 = 0, decimal amount2 = 0,
        decimal amount1 = 0, decimal coinAmount = 0)
    {
        ValidateAmount(amount1000, nameof(amount1000));
        ValidateAmount(amount500, nameof(amount500));
        ValidateAmount(amount200, nameof(amount200));
        ValidateAmount(amount100, nameof(amount100));
        ValidateAmount(amount50, nameof(amount50));
        ValidateAmount(amount20, nameof(amount20));
        ValidateAmount(amount10, nameof(amount10));
        ValidateAmount(amount5, nameof(amount5));
        ValidateAmount(amount2, nameof(amount2));
        ValidateAmount(amount1, nameof(amount1));
        ValidateAmount(coinAmount, nameof(coinAmount));

        Amount1000 = amount1000;
        Amount500 = amount500;
        Amount200 = amount200;
        Amount100 = amount100;
        Amount50 = amount50;
        Amount20 = amount20;
        Amount10 = amount10;
        Amount5 = amount5;
        Amount2 = amount2;
        Amount1 = amount1;
        CoinAmount = coinAmount;

        UpdateTimestamp();
    }

    public decimal GetTotal()
    {
        return Amount1000 + Amount500 + Amount200 + Amount100 + Amount50 +
               Amount20 + Amount10 + Amount5 + Amount2 + Amount1 + CoinAmount;
    }

    private void ValidateAmount(decimal amount, string paramName)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", paramName);
    }
}
