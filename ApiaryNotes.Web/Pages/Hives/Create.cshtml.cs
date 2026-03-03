using ApiaryNotes.Web.Application.Services;
using ApiaryNotes.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Web.Pages.Hives;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly HiveService hiveService;
    private readonly UserManager<IdentityUser> userManager;

    public CreateModel(ApplicationDbContext db, HiveService hiveService, UserManager<IdentityUser> userManager)
    {
        this.db = db;
        this.hiveService = hiveService;
        this.userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int ApiaryId { get; set; }

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
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = userManager.GetUserId(User)!;

        // saugumas + kad turėtume ApiaryName, jei reiks grąžinti Page() su klaida
        var apiary = await db.Apiaries.AsNoTracking().FirstOrDefaultAsync(a => a.Id == ApiaryId);
        if (apiary is null) return NotFound();
        if (apiary.OwnerUserId != userId) return Forbid();

        ApiaryName = apiary.Name;

        if (string.IsNullOrWhiteSpace(Input.Code))
        {
            ModelState.AddModelError(nameof(Input.Code), "Pavadinimas privalomas.");
            return Page();
        }

        try
        {
            await hiveService.CreateAsync(userId, ApiaryId, Input.Code, Input.HiveType);
            return RedirectToPage("Index", new { apiaryId = ApiaryId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(Input.Code), ex.Message);
            return Page();
        }
    }
}