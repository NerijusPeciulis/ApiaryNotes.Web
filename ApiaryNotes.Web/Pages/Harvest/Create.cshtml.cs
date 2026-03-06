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
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly HarvestService harvestService;
    private readonly UserManager<IdentityUser> userManager;

    public CreateModel(ApplicationDbContext db, HarvestService harvestService, UserManager<IdentityUser> userManager)
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

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public decimal Amount { get; set; }
        public HarvestUnit Unit { get; set; } = HarvestUnit.Kg;
        public string? Note { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User)!;

        var apiary = await db.Apiaries.AsNoTracking().FirstOrDefaultAsync(a => a.Id == ApiaryId);
        if (apiary is null) return NotFound();
        if (apiary.OwnerUserId != userId) return Forbid();
        ApiaryName = apiary.Name;

        var hive = await db.Hives.AsNoTracking().FirstOrDefaultAsync(h => h.Id == HiveId);
        if (hive is null) return NotFound();
        if (hive.OwnerUserId != userId) return Forbid();
        if (hive.ApiaryId != ApiaryId) return NotFound();
        HiveCode = hive.Code;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = userManager.GetUserId(User)!;

        if (Input.Amount <= 0)
        {
            ModelState.AddModelError(nameof(Input.Amount), "Kiekis turi būti > 0.");
            return Page();
        }

        await harvestService.CreateAsync(userId, HiveId, Input.Date, Input.Amount, Input.Unit, Input.Note);
        return RedirectToPage("Index", new { apiaryId = ApiaryId, hiveId = HiveId });
    }
}