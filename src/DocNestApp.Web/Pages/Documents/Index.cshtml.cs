using DocNestApp.Contracts.Documents;
using DocNestApp.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocNestApp.Web.Pages.Documents;

public sealed class IndexModel(DocNestApiClient api) : PageModel
{
    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 20;

    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? Type { get; set; }

    [BindProperty(SupportsGet = true)] public DateOnly? ExpiresAfter { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? ExpiresBefore { get; set; }
    [BindProperty(SupportsGet = true)] public bool IncludeNoExpiry { get; set; }

    public ListDocumentsResponse Result { get; private set; }
        = new(1, 20, 0, Array.Empty<DocumentDto>());

    public async Task OnGet(CancellationToken ct)
    {
        var req = new ListDocumentsRequest(
            Page: Page <= 0 ? 1 : Page,
            PageSize: PageSize <= 0 ? 20 : Math.Min(PageSize, 50),
            Q: Q,
            Type: Type,
            ExpiresAfter: ExpiresAfter,
            ExpiresBefore: ExpiresBefore,
            IncludeNoExpiry: IncludeNoExpiry);

        Result = await api.ListDocumentsAsync(req, ct);
    }
}