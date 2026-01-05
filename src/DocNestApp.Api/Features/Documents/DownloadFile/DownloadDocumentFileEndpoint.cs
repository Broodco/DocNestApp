using DocNestApp.Application.Abstractions.Storage;

namespace DocNestApp.Api.Features.Documents.DownloadFile;

using FastEndpoints;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

public sealed class DownloadDocumentFileEndpoint(AppDbContext db, IFileStore fileStore)
    : Endpoint<DownloadDocumentFileRequest>
{
    public override void Configure()
    {
        Get("/documents/{id:guid}/file");
        AllowAnonymous(); // MVP
    }

    public override async Task HandleAsync(DownloadDocumentFileRequest req, CancellationToken ct)
    {
        var userId = DevUser.UserId;

        var doc = await db.Documents
            .AsNoTracking()
            .Where(d => d.Id == req.Id && d.UserId == userId)
            .Select(d => new { d.Id, d.FileKey, d.OriginalFileName, d.ContentType })
            .SingleOrDefaultAsync(ct);

        if (doc is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (doc.FileKey is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var stored = await fileStore.OpenReadAsync(userId, doc.Id, ct);
        if (stored is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.StreamAsync(
            stored.Content,
            fileName: doc.OriginalFileName ?? stored.OriginalFileName,
            fileLengthBytes: stored.SizeBytes,
            contentType: doc.ContentType ?? stored.ContentType,
            cancellation: ct);
    }
}

public sealed class DownloadDocumentFileRequest
{
    public Guid Id { get; init; }
}