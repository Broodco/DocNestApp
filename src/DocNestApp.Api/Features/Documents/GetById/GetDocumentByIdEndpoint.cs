using DocNestApp.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace DocNestApp.Api.Features.Documents.GetById;

public sealed class GetDocumentByIdEndpoint(AppDbContext db) : Endpoint<GetDocumentByIdRequest, GetDocumentByIdResponse>
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
            .Select(d => new GetDocumentByIdResponse
            {
                Id = d.Id,
                Title = d.Title,
                Type = d.Type,
                ExpiresOn = d.ExpiresOn
            })
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

public sealed class GetDocumentByIdResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string Type { get; init; } = null!;
    public DateOnly? ExpiresOn { get; init; }
}