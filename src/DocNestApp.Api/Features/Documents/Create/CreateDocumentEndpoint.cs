namespace DocNestApp.Api.Features.Documents.Create;

using Domain.Documents;
using Infrastructure.Database;
using FastEndpoints;
using FluentValidation;

public sealed class CreateDocumentEndpoint(AppDbContext db) : Endpoint<CreateDocumentRequest, CreateDocumentResponse>
{
    public override void Configure()
    {
        Post("/documents");
        AllowAnonymous();
        Validator<CreateDocumentValidator>();
        
        Description(d =>
        {
            d.Produces<CreateDocumentResponse>(StatusCodes.Status201Created);
            d.Produces(StatusCodes.Status400BadRequest);
            d.Produces(StatusCodes.Status500InternalServerError);
        });

        Summary(s =>
        {
            s.Summary = "Create a document";
            s.Description = "Creates a document and returns its id. Sets Location header to the new resource URL.";
        });
    }

    public override async Task HandleAsync(CreateDocumentRequest req, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var doc = Document.Create(
            userId: DevUser.UserId,
            subjectId: DevUser.SubjectId,
            title: req.Title,
            type: req.Type,
            expiresOn: req.ExpiresOn,
            utcNow: now);

        db.Documents.Add(doc);
        await db.SaveChangesAsync(ct);
        
        var location = $"/documents/{doc.Id}";
        HttpContext.Response.Headers.Location = location;
        HttpContext.Response.StatusCode = StatusCodes.Status201Created;

        await Send.OkAsync(new CreateDocumentResponse { Id = doc.Id }, ct);
    }
}

public sealed class CreateDocumentRequest
{
    public string Title { get; init; } = null!;
    public string Type { get; init; } = null!;
    public DateOnly? ExpiresOn { get; init; }
}

public sealed class CreateDocumentResponse
{
    public Guid Id { get; init; }
}

public sealed class CreateDocumentValidator : AbstractValidator<CreateDocumentRequest>
{
    public CreateDocumentValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
    }
}
