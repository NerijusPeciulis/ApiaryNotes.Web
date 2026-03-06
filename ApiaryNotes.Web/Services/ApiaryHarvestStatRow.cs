namespace ApiaryNotes.Web.Application.Services;

public sealed class ApiaryHarvestStatRow
{
    public int HiveId { get; set; }
    public string HiveCode { get; set; } = string.Empty;
    public decimal TotalKg { get; set; }
    public decimal TotalL { get; set; }
}