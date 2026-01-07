using DocNestApp.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocNestApp.Web.Pages;

public sealed class IndexModel(DocNestApiClient api, IConfiguration config, IHostEnvironment env) : PageModel
{
    public string? ApiBaseUrl { get; private set; }
    public string? SwaggerUrl { get; private set; }

    [TempData] public string? Toast { get; set; }

    public bool IsDevelopment { get; private set; }

    public void OnGet()
    {
        IsDevelopment = env.IsDevelopment();
        ApiBaseUrl = config["DocNest:ApiBaseUrl"];

        if (!string.IsNullOrWhiteSpace(ApiBaseUrl))
            SwaggerUrl = new Uri(new Uri(ApiBaseUrl), "/swagger").ToString();
    }

    public async Task<IActionResult> OnPostResetDemoAsync(CancellationToken ct)
    {
        if (!env.IsDevelopment())
            return Forbid();

        await api.ResetDemoAsync(ct);

        Toast = "Demo data reset ✅";
        return RedirectToPage("/Index");
    }
}