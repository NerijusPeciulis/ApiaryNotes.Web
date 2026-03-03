using ApiaryNotes.Web.Application.Services;
using ApiaryNotes.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Web.Pages.Notes;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly NoteService noteService;
    private readonly UserManager<IdentityUser> userManager;

    public EditModel(ApplicationDbContext db, NoteService noteService, UserManager<IdentityUser> userManager)
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
    public int Id { get; set; } // noteId

    public string ApiaryName { get; private set; } = string.Empty;
    public string HiveCode { get; private set; } = string.Empty;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public DateOnly Date { get; set; }
        public string? Title { get; set; }
        public string Text { get; set; } = string.Empty;
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

        var note = await db.HiveNotes.AsNoTracking().FirstOrDefaultAsync(n => n.Id == Id);
        if (note is null) return NotFound();
        if (note.OwnerUserId != userId) return Forbid();
        if (note.HiveId != HiveId) return NotFound();

        Input = new InputModel { Date = note.Date, Title = note.Title, Text = note.Text };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = userManager.GetUserId(User)!;

        if (string.IsNullOrWhiteSpace(Input.Text))
        {
            ModelState.AddModelError(nameof(Input.Text), "Tekstas privalomas.");
            return Page();
        }

        await noteService.UpdateAsync(Id, userId, Input.Date, Input.Title, Input.Text);
        return RedirectToPage("Index", new { apiaryId = ApiaryId, hiveId = HiveId });
    }
}