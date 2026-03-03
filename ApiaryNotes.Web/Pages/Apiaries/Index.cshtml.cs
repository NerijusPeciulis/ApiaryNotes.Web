using ApiaryNotes.Web.Application.Services;
using ApiaryNotes.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiaryNotes.Web.Pages.Apiaries;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApiaryService apiaryService;
    private readonly UserManager<IdentityUser> userManager;

    public IndexModel(ApiaryService apiaryService, UserManager<IdentityUser> userManager)
    {
        this.apiaryService = apiaryService;
        this.userManager = userManager;
    }

    public List<Apiary> Apiaries { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var userId = userManager.GetUserId(User)!;
        Apiaries = await apiaryService.GetForUserAsync(userId);
    }
}