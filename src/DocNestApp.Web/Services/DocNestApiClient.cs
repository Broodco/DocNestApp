using System.Net.Http.Json;
using DocNestApp.Contracts.Documents;

namespace DocNestApp.Web.Services;

public sealed class DocNestApiClient(HttpClient http)
{
    public async Task<ListDocumentsResponse> ListDocumentsAsync(ListDocumentsRequest req, CancellationToken ct)
    {
        var qs = new List<string>
        {
            $"page={req.Page}",
            $"pageSize={req.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(req.Q))
            qs.Add($"q={Uri.EscapeDataString(req.Q)}");

        if (!string.IsNullOrWhiteSpace(req.Type))
            qs.Add($"type={Uri.EscapeDataString(req.Type)}");

        if (req.ExpiresAfter is not null)
            qs.Add($"expiresAfter={req.ExpiresAfter:yyyy-MM-dd}");

        if (req.ExpiresBefore is not null)
            qs.Add($"expiresBefore={req.ExpiresBefore:yyyy-MM-dd}");

        if (req.IncludeNoExpiry)
            qs.Add("includeNoExpiry=true");

        var url = "/documents" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");

        return await http.GetFromJsonAsync<ListDocumentsResponse>(url, ct)
               ?? new ListDocumentsResponse(req.Page, req.PageSize, 0, Array.Empty<DocumentDto>());
    }

    public async Task<Guid> CreateDocumentAsync(
        string title,
        string type,
        DateOnly? expiresOn,
        IFormFile? file,
        CancellationToken ct)
    {
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(title), "title");
        content.Add(new StringContent(type), "type");

        if (expiresOn is not null)
            content.Add(new StringContent(expiresOn.Value.ToString("yyyy-MM-dd")), "expiresOn");

        if (file is not null && file.Length > 0)
        {
            var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "file", file.FileName);
        }

        var res = await http.PostAsync("/documents", content, ct);
        res.EnsureSuccessStatusCode();

        var body = await res.Content.ReadFromJsonAsync<CreateDocumentResponse>(ct);
        return body!.Id;
    }
    
    public async Task<GetDocumentResponse?> GetDocumentAsync(Guid id, CancellationToken ct)
    {
        var res = await http.GetAsync($"/documents/{id}", ct);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<GetDocumentResponse>(ct);
    }

    public async Task ResetDemoAsync(CancellationToken ct)
    {
        var res = await http.PostAsync("/dev/reset-demo", content: null, ct);
        res.EnsureSuccessStatusCode();
    }

    public async Task<(Stream Content, string ContentType, string FileName)> DownloadFileAsync(Guid documentId, CancellationToken ct)
    {
        var res = await http.GetAsync($"/documents/{documentId}/file", HttpCompletionOption.ResponseHeadersRead, ct);

        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            throw new FileNotFoundException("File not found for document " + documentId);

        res.EnsureSuccessStatusCode();

        var contentType = res.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

        string fileName = $"document-{documentId}.bin";
        var cd = res.Content.Headers.ContentDisposition;
        if (cd?.FileNameStar is { Length: > 0 })
            fileName = cd.FileNameStar;
        else if (cd?.FileName is { Length: > 0 })
            fileName = cd.FileName.Trim('"');

        var stream = await res.Content.ReadAsStreamAsync(ct);

        return (stream, contentType, fileName);
    }

}