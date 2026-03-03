using ApiaryNotes.Web.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiaryNotes.Web.Pages.Apiaries;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApiaryService apiaryService;
    private readonly UserManager<IdentityUser> userManager;

    public CreateModel(ApiaryService apiaryService, UserManager<IdentityUser> userManager)
    {
        this.apiaryService = apiaryService;
        this.userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Location { get; set; }
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.Name))
        {
            ModelState.AddModelError(nameof(Input.Name), "Pavadinimas privalomas.");
            return Page();
        }

        var userId = userManager.GetUserId(User)!;
        await apiaryService.CreateAsync(userId, Input.Name, Input.Location);

        return RedirectToPage("Index");
    }
}