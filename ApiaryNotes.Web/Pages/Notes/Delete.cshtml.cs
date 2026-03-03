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
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly NoteService noteService;
    private readonly UserManager<IdentityUser> userManager;

    public DeleteModel(ApplicationDbContext db, NoteService noteService, UserManager<IdentityUser> userManager)
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

    [BindProperty]
    public HiveNote Note { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User)!;

        var note = await db.HiveNotes.AsNoTracking().FirstOrDefaultAsync(n => n.Id == Id);
        if (note is null) return NotFound();
        if (note.OwnerUserId != userId) return Forbid();
        if (note.HiveId != HiveId) return NotFound();

        Note = note;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = userManager.GetUserId(User)!;

        await noteService.DeleteAsync(Id, userId);
        return RedirectToPage("Index", new { apiaryId = ApiaryId, hiveId = HiveId });
    }
}