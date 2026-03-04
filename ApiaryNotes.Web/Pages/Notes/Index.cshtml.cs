using ApiaryNotes.Web.Application.Services;
using ApiaryNotes.Web.Data;
using ApiaryNotes.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Web.Pages.Notes;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly NoteService noteService;
    private readonly UserManager<IdentityUser> userManager;

    public IndexModel(ApplicationDbContext db, NoteService noteService, UserManager<IdentityUser> userManager)
    {
        this.db = db;
        this.noteService = noteService;
        this.userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int ApiaryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int HiveId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public string ApiaryName { get; private set; } = string.Empty;
    public string HiveCode { get; private set; } = string.Empty;

    public List<HiveNote> Notes { get; private set; } = new();

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

        var query = db.HiveNotes
    .AsNoTracking()
    .Where(n => n.HiveId == HiveId && n.OwnerUserId == userId);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim().ToLower();

            query = query.Where(n =>
                (n.Title != null && n.Title.ToLower().Contains(s)) ||
                n.Text.ToLower().Contains(s));
        }

        Notes = await query
            .OrderByDescending(n => n.Date)
            .ThenByDescending(n => n.Id)
            .ToListAsync();
        return Page();
    }
}