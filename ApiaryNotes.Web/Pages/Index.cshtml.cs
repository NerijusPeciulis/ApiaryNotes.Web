using ApiaryNotes.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly UserManager<IdentityUser> userManager;

    public IndexModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        this.db = db;
        this.userManager = userManager;
    }

    public bool IsAuthenticated { get; private set; }

    public int ApiariesCount { get; private set; }
    public int HivesCount { get; private set; }
    public int NotesCount { get; private set; }

    public async Task OnGetAsync()
    {
        IsAuthenticated = User?.Identity?.IsAuthenticated == true;
        if (!IsAuthenticated)
            return;

        var userId = userManager.GetUserId(User)!;

        ApiariesCount = await db.Apiaries.CountAsync(a => a.OwnerUserId == userId);
        HivesCount = await db.Hives.CountAsync(h => h.OwnerUserId == userId);
        NotesCount = await db.HiveNotes.CountAsync(n => n.OwnerUserId == userId);
    }
}