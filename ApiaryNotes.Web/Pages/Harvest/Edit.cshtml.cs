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
public class EditModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly UserManager<IdentityUser> userManager;

    public EditModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        this.db = db;
        this.userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int ApiaryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int HiveId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; } // harvestId

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public DateOnly Date { get; set; }
        public decimal Amount { get; set; }
        public HarvestUnit Unit { get; set; }
        public string? Note { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User)!;

        var item = await db.HiveHarvests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
        if (item is null) return NotFound();
        if (item.OwnerUserId != userId) return Forbid();
        if (item.HiveId != HiveId) return NotFound();

        Input = new InputModel
        {
            Date = item.Date,
            Amount = item.Amount,
            Unit = item.Unit,
            Note = item.Note
        };

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

        var item = await db.HiveHarvests.FirstOrDefaultAsync(x => x.Id == Id);
        if (item is null) return NotFound();
        if (item.OwnerUserId != userId) return Forbid();
        if (item.HiveId != HiveId) return NotFound();

        item.Date = Input.Date;
        item.Amount = Input.Amount;
        item.Unit = Input.Unit;
        item.Note = string.IsNullOrWhiteSpace(Input.Note) ? null : Input.Note.Trim();

        await db.SaveChangesAsync();

        return RedirectToPage("Index", new { apiaryId = ApiaryId, hiveId = HiveId });
    }
}