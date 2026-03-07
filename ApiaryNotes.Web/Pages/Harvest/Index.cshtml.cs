using ApiaryNotes.Web.Application.Services;
using ApiaryNotes.Web.Data;
using ApiaryNotes.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Web.Pages.Harvest;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly HarvestService harvestService;
    private readonly UserManager<IdentityUser> userManager;

    public IndexModel(
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
    public int HiveId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Year { get; set; }

    [BindProperty(SupportsGet = true)]
    public ProductType? Product { get; set; }

    public string ApiaryName { get; private set; } = string.Empty;
    public string HiveCode { get; private set; } = string.Empty;

    public List<HiveHarvest> Items { get; private set; } = new();

    public decimal TotalKg { get; private set; }
    public decimal TotalL { get; private set; }
    public decimal TotalG { get; private set; }

    public List<int> AvailableYears { get; private set; } = new();

    public int SelectedYear { get; private set; }

    public ProductType SelectedProduct { get; private set; } = ProductType.Honey;

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

        var hive = await db.Hives
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == HiveId);

        if (hive is null)
        {
            return NotFound();
        }

        if (hive.OwnerUserId != userId)
        {
            return Forbid();
        }

        if (hive.ApiaryId != ApiaryId)
        {
            return NotFound();
        }

        HiveCode = hive.Code;

        AvailableYears = await harvestService.GetYearsForHiveAsync(userId, HiveId);

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

        Items = (await harvestService.GetForHiveByYearAsync(userId, HiveId, SelectedYear))
            .Where(x => x.Product == SelectedProduct)
            .ToList();

        TotalKg = Items
            .Where(x => x.Unit == HarvestUnit.Kg)
            .Sum(x => x.Amount);

        TotalL = Items
            .Where(x => x.Unit == HarvestUnit.L)
            .Sum(x => x.Amount);

        TotalG = Items
            .Where(x => x.Unit == HarvestUnit.G)
            .Sum(x => x.Amount);

        return Page();
    }
}