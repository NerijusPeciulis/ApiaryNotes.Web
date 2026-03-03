using System.ComponentModel.DataAnnotations;

namespace ApiaryNotes.Web.Domain;

public class Hive
{
    public int Id { get; set; }

    public int ApiaryId { get; set; }
    public Apiary Apiary { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty; // pvz. A1, A2

    [MaxLength(100)]
    public string? HiveType { get; set; } // Dadant, Langstroth...

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Required]
    public string OwnerUserId { get; set; } = string.Empty;

    public List<HiveNote> Notes { get; set; } = new();
}