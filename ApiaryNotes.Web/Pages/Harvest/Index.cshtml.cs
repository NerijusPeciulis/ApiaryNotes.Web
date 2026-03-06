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

    public string ApiaryName { get; private set; } = string.Empty;
    public string HiveCode { get; private set; } = string.Empty;

    public List<HiveHarvest> Items { get; private set; } = new();

    public decimal TotalKg { get; private set; }
    public decimal TotalL { get; private set; }

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

        Items = await harvestService.GetForHiveAsync(userId, HiveId);

        TotalKg = Items
            .Where(x => x.Unit == HarvestUnit.Kg)
            .Sum(x => x.Amount);

        TotalL = Items
            .Where(x => x.Unit == HarvestUnit.L)
            .Sum(x => x.Amount);

        return Page();
    }
}