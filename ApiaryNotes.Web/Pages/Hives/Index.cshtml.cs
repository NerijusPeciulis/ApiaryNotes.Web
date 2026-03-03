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
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly HiveService hiveService;
    private readonly UserManager<IdentityUser> userManager;

    public IndexModel(ApplicationDbContext db, HiveService hiveService, UserManager<IdentityUser> userManager)
    {
        this.db = db;
        this.hiveService = hiveService;
        this.userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int ApiaryId { get; set; }

    public string ApiaryName { get; private set; } = string.Empty;

    public List<Hive> Hives { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User)!;

        var apiary = await db.Apiaries.AsNoTracking().FirstOrDefaultAsync(a => a.Id == ApiaryId);
        if (apiary is null) return NotFound();
        if (apiary.OwnerUserId != userId) return Forbid();

        ApiaryName = apiary.Name;
        Hives = await hiveService.GetForApiaryAsync(userId, ApiaryId);

        return Page();
    }
}