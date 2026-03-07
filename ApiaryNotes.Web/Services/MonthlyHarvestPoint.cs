namespace ApiaryNotes.Web.Application.Services;

public sealed class MonthlyHarvestPoint
{
    public int Month { get; set; }
    public decimal TotalKg { get; set; }
    public decimal TotalL { get; set; }
    public decimal TotalG { get; set; }
}