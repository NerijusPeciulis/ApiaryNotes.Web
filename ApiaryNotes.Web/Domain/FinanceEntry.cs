namespace ApiaryNotes.Web.Domain;

public class FinanceEntry
{
    public int Id { get; set; }

    public string OwnerUserId { get; set; } = string.Empty;

    public int? ApiaryId { get; set; }
    public Apiary? Apiary { get; set; }

    public DateOnly Date { get; set; }

    public FinanceEntryType Type { get; set; }

    public FinanceCategory Category { get; set; }

    public decimal Amount { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Note { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}

public enum FinanceEntryType
{
    Expense = 1,
    Income = 2
}

public enum FinanceCategory
{
    Other = 1,

    Sugar = 10,
    Frames = 11,
    Foundation = 12,
    Medication = 13,
    Equipment = 14,
    Fuel = 15,
    Jars = 16,

    HoneySales = 100,
    WaxSales = 101,
    BeeBreadSales = 102,
    PropolisSales = 103,
    OtherIncome = 104
}