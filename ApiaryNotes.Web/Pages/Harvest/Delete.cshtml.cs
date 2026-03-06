using ApiaryNotes.Web.Application.Services;
using ApiaryNotes.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Web.Pages.Harvest;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly HarvestService harvestService;
    private readonly UserManager<IdentityUser> userManager;

    public DeleteModel(ApplicationDbContext db, HarvestService harvestService, UserManager<IdentityUser> userManager)
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
    public int Id { get; set; } // harvestId

    public string Summary { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User)!;

        var item = await db.HiveHarvests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
        if (item is null) return NotFound();
        if (item.OwnerUserId != userId) return Forbid();
        if (item.HiveId != HiveId) return NotFound();

        Summary = $"{item.Date} – {item.Amount} {item.Unit}";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = userManager.GetUserId(User)!;

        await harvestService.DeleteAsync(Id, userId);
        return RedirectToPage("Index", new { apiaryId = ApiaryId, hiveId = HiveId });
    }
}