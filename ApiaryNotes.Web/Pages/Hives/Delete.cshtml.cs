using ApiaryNotes.Web.Application.Services;
using ApiaryNotes.Web.Data;
using ApiaryNotes.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Web.Pages.Hives;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly HiveService hiveService;
    private readonly UserManager<IdentityUser> userManager;

    public DeleteModel(ApplicationDbContext db, HiveService hiveService, UserManager<IdentityUser> userManager)
    {
        this.db = db;
        this.hiveService = hiveService;
        this.userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int ApiaryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public string ApiaryName { get; private set; } = string.Empty;

    [BindProperty]
    public Hive Hive { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User)!;

        var apiary = await db.Apiaries.AsNoTracking().FirstOrDefaultAsync(a => a.Id == ApiaryId);
        if (apiary is null) return NotFound();
        if (apiary.OwnerUserId != userId) return Forbid();
        ApiaryName = apiary.Name;

        var hive = await db.Hives.AsNoTracking().FirstOrDefaultAsync(h => h.Id == Id);
        if (hive is null) return NotFound();
        if (hive.OwnerUserId != userId) return Forbid();
        if (hive.ApiaryId != ApiaryId) return NotFound();

        Hive = hive;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = userManager.GetUserId(User)!;

        // papildomas check
        var hive = await db.Hives.AsNoTracking().FirstOrDefaultAsync(h => h.Id == Id);
        if (hive is null) return NotFound();
        if (hive.OwnerUserId != userId) return Forbid();

        await hiveService.DeleteAsync(Id, userId);
        return RedirectToPage("Index", new { apiaryId = ApiaryId });
    }
}