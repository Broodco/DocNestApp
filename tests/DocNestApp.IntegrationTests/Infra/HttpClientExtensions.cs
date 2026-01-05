namespace DocNestApp.IntegrationTests.Infra;

using System.Net.Http.Headers;
using System.Text;

public static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> PostDocumentMultipartAsync(
        this HttpClient client,
        string url,
        string title,
        string type,
        string? expiresOn = null,
        (string FileName, string ContentType, byte[] Content)? file = null)
    {
        var form = new MultipartFormDataContent();

        form.Add(new StringContent(title, Encoding.UTF8), "title");
        form.Add(new StringContent(type, Encoding.UTF8), "type");

        if (!string.IsNullOrWhiteSpace(expiresOn))
            form.Add(new StringContent(expiresOn, Encoding.UTF8), "expiresOn");

        if (file is not null)
        {
            var bytes = new ByteArrayContent(file.Value.Content);
            bytes.Headers.ContentType = new MediaTypeHeaderValue(file.Value.ContentType);
            form.Add(bytes, "file", file.Value.FileName);
        }

        return client.PostAsync(url, form);
    }
}