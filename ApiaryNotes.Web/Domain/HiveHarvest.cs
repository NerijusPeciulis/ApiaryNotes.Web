namespace ApiaryNotes.Web.Domain;

public class HiveHarvest
{
    public int Id { get; set; }

    public string OwnerUserId { get; set; } = string.Empty;

    public int HiveId { get; set; }
    public Hive Hive { get; set; } = null!;

    public DateOnly Date { get; set; }

    public decimal Amount { get; set; } // kiekis

    public HarvestUnit Unit { get; set; } // Kg arba L

    public string? Note { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}

public enum HarvestUnit
{
    Kg = 1,
    L = 2,
}