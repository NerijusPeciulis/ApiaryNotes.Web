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
    private readonly UserManager<IdentityUser> userManager;

    public IndexModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        this.db = db;
        this.userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int ApiaryId { get; set; }

    public string ApiaryName { get; private set; } = string.Empty;

    public List<HiveListItem> Hives { get; private set; } = new();

    public class HiveListItem
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? HiveType { get; set; }
        public decimal TotalKg { get; set; }
        public decimal TotalL { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User)!;

        var apiary = await db.Apiaries
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == ApiaryId);

        if (apiary is null)
        {
            return NotFound();
        }

        if (apiary.OwnerUserId != userId)
        {
            return Forbid();
        }

        ApiaryName = apiary.Name;

        var hives = await db.Hives
            .AsNoTracking()
            .Where(h => h.ApiaryId == ApiaryId && h.OwnerUserId == userId)
            .OrderBy(h => h.Code)
            .ToListAsync();

        var harvests = await db.HiveHarvests
            .AsNoTracking()
            .Where(x => x.OwnerUserId == userId)
            .ToListAsync();

        Hives = hives.Select(h =>
        {
            var hiveHarvest = harvests.Where(x => x.HiveId == h.Id);

            return new HiveListItem
            {
                Id = h.Id,
                Code = h.Code,
                HiveType = h.HiveType,
                TotalKg = hiveHarvest
                    .Where(x => x.Unit == HarvestUnit.Kg)
                    .Sum(x => x.Amount),
                TotalL = hiveHarvest
                    .Where(x => x.Unit == HarvestUnit.L)
                    .Sum(x => x.Amount)
            };
        }).ToList();

        return Page();
    }
}