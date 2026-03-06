namespace ApiaryNotes.Web.Domain;

public class HiveHarvest
{
    public int Id { get; set; }

    public string OwnerUserId { get; set; } = string.Empty;

    public int HiveId { get; set; }
    public Hive Hive { get; set; } = null!;

    public DateOnly Date { get; set; }

    public ProductType Product { get; set; } = ProductType.Honey;

    public decimal Amount { get; set; }

    public HarvestUnit Unit { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}

public enum HarvestUnit
{
    Kg = 1,
    L = 2,
}

public enum ProductType
{
    Honey = 1,
    Wax = 2,
    BeeBread = 3,
    Propolis = 4,
}