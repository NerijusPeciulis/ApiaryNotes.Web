using System.ComponentModel.DataAnnotations;

namespace ApiaryNotes.Web.Domain;

public class Apiary
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Location { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Required]
    public string OwnerUserId { get; set; } = string.Empty;

    public List<Hive> Hives { get; set; } = new();
}