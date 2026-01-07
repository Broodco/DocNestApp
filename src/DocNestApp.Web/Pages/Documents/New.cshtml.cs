using DocNestApp.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocNestApp.Web.Pages.Documents;

public sealed class NewModel(DocNestApiClient api) : PageModel
{
    [BindProperty] public string Title { get; set; } = "";
    [BindProperty] public string Type { get; set; } = "";
    [BindProperty] public DateOnly? ExpiresOn { get; set; }
    [BindProperty] public IFormFile? File { get; set; }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var id = await api.CreateDocumentAsync(
            Title,
            Type,
            ExpiresOn,
            File,
            ct);

        return RedirectToPage("/Documents/Details", new { id });
    }
}