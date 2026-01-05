using DocNestApp.Application.Abstractions.Storage;
using Microsoft.Extensions.Options;

namespace DocNestApp.Infrastructure.Files;

public sealed class LocalFileStore : IFileStore
{
    private readonly FileStoreOptions _options;

    public LocalFileStore(IOptions<FileStoreOptions> options)
    {
        _options = options.Value;
    }

    public async Task<StoredFileInfo> SaveAsync(
        Guid userId,
        Guid documentId,
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken ct)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (documentId == Guid.Empty) throw new ArgumentException("DocumentId is required.", nameof(documentId));
        if (content is null) throw new ArgumentNullException(nameof(content));
        if (string.IsNullOrWhiteSpace(originalFileName)) throw new ArgumentException("OriginalFileName is required.", nameof(originalFileName));
        if (string.IsNullOrWhiteSpace(contentType)) contentType = "application/octet-stream";

        var safeName = SanitizeFileName(originalFileName);
        var ext = Path.GetExtension(safeName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".bin";

        // Keep it deterministic: one "current" file per document
        var fileKey = $"{documentId}{ext}".ToLowerInvariant();

        var docDir = GetDocumentDirectory(userId, documentId);
        Directory.CreateDirectory(docDir);

        var fullPath = Path.Combine(docDir, fileKey);

        // Overwrite allowed (supports replace)
        await using (var fs = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 64 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await content.CopyToAsync(fs, ct);
        }

        var info = new FileInfo(fullPath);

        return new StoredFileInfo(
            FileKey: fileKey,
            OriginalFileName: safeName,
            ContentType: contentType,
            SizeBytes: info.Length);
    }

    public Task<StoredFile?> OpenReadAsync(Guid userId, Guid documentId, CancellationToken ct)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (documentId == Guid.Empty) throw new ArgumentException("DocumentId is required.", nameof(documentId));

        var docDir = GetDocumentDirectory(userId, documentId);

        if (!Directory.Exists(docDir))
            return Task.FromResult<StoredFile?>(null);

        // MVP simplification: there is exactly one current file in the folder
        var file = Directory.EnumerateFiles(docDir)
            .Select(p => new FileInfo(p))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .FirstOrDefault();

        if (file is null || !file.Exists)
            return Task.FromResult<StoredFile?>(null);

        // If you later want persisted metadata: use DB fields instead of guessing
        var contentType = GuessContentType(file.Extension);
        var originalName = file.Name; // fallback; typically you'd store OriginalFileName in DB

        Stream stream = new FileStream(
            file.FullName,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 64 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        StoredFile result = new(
            Content: stream,
            OriginalFileName: originalName,
            ContentType: contentType,
            SizeBytes: file.Length);

        return Task.FromResult<StoredFile?>(result);
    }

    public Task DeleteAsync(Guid userId, Guid documentId, CancellationToken ct)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (documentId == Guid.Empty) throw new ArgumentException("DocumentId is required.", nameof(documentId));

        var docDir = GetDocumentDirectory(userId, documentId);

        if (Directory.Exists(docDir))
            Directory.Delete(docDir, recursive: true);

        return Task.CompletedTask;
    }

    private string GetDocumentDirectory(Guid userId, Guid documentId)
    {
        // Strong isolation: userId in the path
        var root = Path.GetFullPath(_options.RootPath);
        return Path.Combine(root, "files", userId.ToString("N"), documentId.ToString("N"));
    }

    private static string SanitizeFileName(string fileName)
    {
        fileName = Path.GetFileName(fileName); // strips any path components
        foreach (var c in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(c, '_');
        return fileName.Trim();
    }

    private static string GuessContentType(string extension)
    {
        extension = extension.Trim().ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}
