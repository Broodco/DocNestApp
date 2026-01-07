using DocNestApp.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocNestApp.Web.Pages.Documents;

public sealed class FileModel(DocNestApiClient api) : PageModel
{
    public async Task<IActionResult> OnGet(Guid id, CancellationToken ct)
    {
        try
        {
            var (content, contentType, fileName) = await api.DownloadFileAsync(id, ct);

            return File(content, contentType, fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }
}