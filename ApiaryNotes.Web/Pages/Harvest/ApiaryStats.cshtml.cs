using ApiaryNotes.Web.Application.Services;
using ApiaryNotes.Web.Data;
using ApiaryNotes.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ApiaryNotes.Web.Pages.Harvest;

[Authorize]
public class ApiaryStatsModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly HarvestService harvestService;
    private readonly UserManager<IdentityUser> userManager;

    public ApiaryStatsModel(
        ApplicationDbContext db,
        HarvestService harvestService,
        UserManager<IdentityUser> userManager)
    {
        this.db = db;
        this.harvestService = harvestService;
        this.userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int ApiaryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Year { get; set; }

    [BindProperty(SupportsGet = true)]
    public ProductType? Product { get; set; }

    public string ApiaryName { get; private set; } = string.Empty;

    public List<int> AvailableYears { get; private set; } = new();

    public int SelectedYear { get; private set; }

    public ProductType SelectedProduct { get; private set; } = ProductType.Honey;

    public decimal TotalKg { get; private set; }
    public decimal TotalL { get; private set; }
    public decimal TotalG { get; private set; }

    public List<ApiaryHarvestStatRow> Rows { get; private set; } = new();

    public List<MonthlyHarvestPoint> MonthlyPoints { get; private set; } = new();

    public string MonthlyLabelsJson { get; private set; } = "[]";
    public string MonthlyKgDataJson { get; private set; } = "[]";
    public string MonthlyLDataJson { get; private set; } = "[]";
    public string MonthlyGDataJson { get; private set; } = "[]";

    public ApiaryHarvestStatRow? BestHive =>
        Rows.OrderByDescending(x => x.TotalKg).FirstOrDefault();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User)!;

        var apiary = await db.Apiaries
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == ApiaryId);

        if (apiary is null)
        {
            return NotFound();
        }

        if (apiary.OwnerUserId != userId)
        {
            return Forbid();
        }

        ApiaryName = apiary.Name;

        AvailableYears = await harvestService.GetYearsForApiaryAsync(userId, ApiaryId);

        if (AvailableYears.Count == 0)
        {
            SelectedYear = DateTime.Now.Year;
            AvailableYears.Add(SelectedYear);
        }
        else
        {
            SelectedYear = Year.HasValue && AvailableYears.Contains(Year.Value)
                ? Year.Value
                : AvailableYears.First();
        }

        SelectedProduct = Product ?? ProductType.Honey;

        var totals = await harvestService.GetTotalsForApiaryByYearAndProductAsync(
            userId,
            ApiaryId,
            SelectedYear,
            SelectedProduct);

        TotalKg = totals.kg;
        TotalL = totals.l;
        TotalG = totals.g;

        Rows = await harvestService.GetApiaryStatsByYearAndProductAsync(
            userId,
            ApiaryId,
            SelectedYear,
            SelectedProduct);

        MonthlyPoints = await harvestService.GetMonthlyTotalsForApiaryByYearAndProductAsync(
            userId,
            ApiaryId,
            SelectedYear,
            SelectedProduct);

        var monthLabels = new[]
        {
            "Sausis", "Vasaris", "Kovas", "Balandis", "Gegužė", "Birželis",
            "Liepa", "Rugpjūtis", "Rugsėjis", "Spalis", "Lapkritis", "Gruodis"
        };

        MonthlyLabelsJson = JsonSerializer.Serialize(monthLabels);
        MonthlyKgDataJson = JsonSerializer.Serialize(MonthlyPoints.Select(x => x.TotalKg));
        MonthlyLDataJson = JsonSerializer.Serialize(MonthlyPoints.Select(x => x.TotalL));
        MonthlyGDataJson = JsonSerializer.Serialize(MonthlyPoints.Select(x => x.TotalG));

        return Page();
    }
}