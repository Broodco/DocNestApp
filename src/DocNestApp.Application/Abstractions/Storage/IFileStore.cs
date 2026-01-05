namespace DocNestApp.Application.Abstractions.Storage;

public interface IFileStore
{
    Task<StoredFileInfo> SaveAsync(
        Guid userId,
        Guid documentId,
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken ct);

    Task<StoredFile?> OpenReadAsync(
        Guid userId,
        Guid documentId,
        CancellationToken ct);

    Task DeleteAsync(
        Guid userId,
        Guid documentId,
        CancellationToken ct);
}

public sealed record StoredFileInfo(
    string FileKey,
    string OriginalFileName,
    string ContentType,
    long SizeBytes);

public sealed record StoredFile(
    Stream Content,
    string OriginalFileName,
    string ContentType,
    long SizeBytes);