using ApiaryNotes.Web.Application.Services;
using ApiaryNotes.Web.Data;
using ApiaryNotes.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Web.Pages.Finance;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext db;
    private readonly FinanceService financeService;
    private readonly UserManager<IdentityUser> userManager;

    public CreateModel(
        ApplicationDbContext db,
        FinanceService financeService,
        UserManager<IdentityUser> userManager)
    {
        this.db = db;
        this.financeService = financeService;
        this.userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> ApiaryOptions { get; private set; } = new();

    public class InputModel
    {
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public FinanceEntryType Type { get; set; } = FinanceEntryType.Expense;
        public FinanceCategory Category { get; set; } = FinanceCategory.Other;
        public decimal Amount { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Note { get; set; }
        public int? ApiaryId { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadApiariesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = userManager.GetUserId(User)!;

        await LoadApiariesAsync();

        if (Input.Amount <= 0)
        {
            ModelState.AddModelError(nameof(Input.Amount), "Suma turi būti > 0.");
        }

        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError(nameof(Input.Title), "Pavadinimas yra privalomas.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await financeService.CreateAsync(
            userId,
            Input.ApiaryId,
            Input.Date,
            Input.Type,
            Input.Category,
            Input.Amount,
            Input.Title,
            Input.Note);

        return RedirectToPage("Index");
    }

    private async Task LoadApiariesAsync()
    {
        var userId = userManager.GetUserId(User)!;

        ApiaryOptions = await db.Apiaries
            .AsNoTracking()
            .Where(x => x.OwnerUserId == userId)
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();

        ApiaryOptions.Insert(0, new SelectListItem
        {
            Value = "",
            Text = "Visam ūkiui"
        });
    }
}