using ApiaryNotes.Web.Application.Services;
using ApiaryNotes.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiaryNotes.Web.Pages.Finance;

[Authorize]
public class IndexModel : PageModel
{
    private readonly FinanceService financeService;
    private readonly UserManager<IdentityUser> userManager;

    public IndexModel(
        FinanceService financeService,
        UserManager<IdentityUser> userManager)
    {
        this.financeService = financeService;
        this.userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int? Year { get; set; }

    public int SelectedYear { get; private set; }

    public List<int> AvailableYears { get; private set; } = new();

    public List<FinanceEntry> Items { get; private set; } = new();

    public decimal TotalIncome { get; private set; }
    public decimal TotalExpense { get; private set; }
    public decimal Profit { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = userManager.GetUserId(User)!;

        AvailableYears = await financeService.GetYearsAsync(userId);

        if (AvailableYears.Count == 0)
        {
            SelectedYear = DateTime.Now.Year;
            AvailableYears.Add(SelectedYear);
        }
        else
        {
            SelectedYear = Year.HasValue && AvailableYears.Contains(Year.Value)
                ? Year.Value
                : AvailableYears.First();
        }

        Items = await financeService.GetByYearAsync(userId, SelectedYear);

        var summary = await financeService.GetSummaryByYearAsync(userId, SelectedYear);
        TotalIncome = summary.income;
        TotalExpense = summary.expense;
        Profit = summary.profit;

        return Page();
    }
}