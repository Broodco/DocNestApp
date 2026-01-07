using DocNestApp.Contracts.Documents;
using DocNestApp.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocNestApp.Web.Pages.Documents;

public sealed class DetailsModel(DocNestApiClient api) : PageModel
{
    public DocumentDto? Document { get; private set; }

    public async Task<IActionResult> OnGet(Guid id, CancellationToken ct)
    {
        var res = await api.GetDocumentAsync(id, ct);
        if (res is null)
            return NotFound();

        Document = res.Document;
        return Page();
    }
}