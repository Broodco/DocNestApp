using DocNestApp.Contracts.Documents;
using DocNestApp.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace DocNestApp.Api.Features.Documents.GetById;

public sealed class GetDocumentByIdEndpoint(AppDbContext db) : Endpoint<GetDocumentByIdRequest, GetDocumentResponse>
{
    public override void Configure()
    {
        Get("/documents/{id:guid}");
        AllowAnonymous(); // MVP
    }
    
    public override async Task HandleAsync(GetDocumentByIdRequest req, CancellationToken ct)
    {
        var userId = DevUser.UserId;

        var doc = await db.Documents
            .AsNoTracking()
            .Where(d => d.Id == req.Id && d.UserId == userId)
            .Select(d => new GetDocumentResponse(
                new DocumentDto(
                    d.Id, 
                    d.UserId, 
                    d.SubjectId, 
                    d.Title, 
                    d.Type, 
                    d.ExpiresOn, 
                    d.CreatedAt, 
                    d.UpdatedAt, 
                    d.FileKey, 
                    d.OriginalFileName, 
                    d.ContentType, 
                    d.SizeBytes)))
            .SingleOrDefaultAsync(ct);

        if (doc is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(doc, ct);
    }
}

public sealed class GetDocumentByIdRequest
{
    public Guid Id { get; init; }
}
