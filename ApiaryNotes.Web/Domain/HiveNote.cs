using System.ComponentModel.DataAnnotations;

namespace ApiaryNotes.Web.Domain;

public class HiveNote
{
    public int Id { get; set; }

    public int HiveId { get; set; }
    public Hive Hive { get; set; } = null!;

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [MaxLength(200)]
    public string? Title { get; set; }

    [Required, MaxLength(4000)]
    public string Text { get; set; } = string.Empty;

    [Required]
    public string OwnerUserId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}