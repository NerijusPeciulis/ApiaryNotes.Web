using ApiaryNotes.Web.Application.Services;
using ApiaryNotes.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Web.Pages.Hives;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly HiveService hiveService;
    private readonly UserManager<IdentityUser> userManager;

    public EditModel(ApplicationDbContext db, HiveService hiveService, UserManager<IdentityUser> userManager)
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
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public string Code { get; set; } = string.Empty;
        public string? HiveType { get; set; }
    }

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

        Input = new InputModel { Code = hive.Code, HiveType = hive.HiveType };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = userManager.GetUserId(User)!;

        var apiary = await db.Apiaries.AsNoTracking().FirstOrDefaultAsync(a => a.Id == ApiaryId);
        if (apiary is null) return NotFound();
        if (apiary.OwnerUserId != userId) return Forbid();
        ApiaryName = apiary.Name;

        if (string.IsNullOrWhiteSpace(Input.Code))
        {
            ModelState.AddModelError(nameof(Input.Code), "Pavadinimas/Kodas privalomas.");
            return Page();
        }

        try
        {
            await hiveService.UpdateAsync(Id, userId, Input.Code, Input.HiveType);
            return RedirectToPage("Index", new { apiaryId = ApiaryId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(Input.Code), ex.Message);
            return Page();
        }
    }
}