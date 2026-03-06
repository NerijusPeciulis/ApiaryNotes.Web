using ApiaryNotes.Web.Application.Services;
using ApiaryNotes.Web.Data;
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

    public string ApiaryName { get; private set; } = string.Empty;

    public List<int> AvailableYears { get; private set; } = new();

    public int SelectedYear { get; private set; }

    public decimal TotalKg { get; private set; }
    public decimal TotalL { get; private set; }

    public List<ApiaryHarvestStatRow> Rows { get; private set; } = new();

    public List<MonthlyHarvestPoint> MonthlyPoints { get; private set; } = new();

    public string MonthlyLabelsJson { get; private set; } = "[]";

    public string MonthlyKgDataJson { get; private set; } = "[]";

    public string MonthlyLDataJson { get; private set; } = "[]";

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

        var totals = await harvestService.GetTotalsForApiaryByYearAsync(userId, ApiaryId, SelectedYear);
        TotalKg = totals.kg;
        TotalL = totals.l;

        Rows = await harvestService.GetApiaryStatsByYearAsync(userId, ApiaryId, SelectedYear);

        MonthlyPoints = await harvestService.GetMonthlyTotalsForApiaryAsync(userId, ApiaryId, SelectedYear);

        var monthLabels = new[]
        {
    "Sausis", "Vasaris", "Kovas", "Balandis", "Gegužė", "Birželis",
    "Liepa", "Rugpjūtis", "Rugsėjis", "Spalis", "Lapkritis", "Gruodis"
};

        MonthlyLabelsJson = JsonSerializer.Serialize(monthLabels);
        MonthlyKgDataJson = JsonSerializer.Serialize(MonthlyPoints.Select(x => x.TotalKg));
        MonthlyLDataJson = JsonSerializer.Serialize(MonthlyPoints.Select(x => x.TotalL));

        return Page();
    }
}