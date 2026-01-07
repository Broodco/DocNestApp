using DocNestApp.Application.Abstractions.Storage;
using DocNestApp.Contracts.Documents;

namespace DocNestApp.Api.Features.Documents.Create;

using Domain.Documents;
using Infrastructure.Database;
using FastEndpoints;
using FluentValidation;

public sealed class CreateDocumentEndpoint(AppDbContext db, IFileStore fileStore) : Endpoint<CreateDocumentRequest, CreateDocumentResponse>
{
    public override void Configure()
    {
        Post("/documents");
        AllowAnonymous();
        AllowFileUploads();
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
            s.Description =
                "Creates a document with metadata and an optional file.\n\n" +
                "Form fields:\n" +
                "- title (string)\n" +
                "- type (string)\n" +
                "- expiresOn (yyyy-MM-dd, optional)\n" +
                "- file (binary, optional)";
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
        
        if (req.File is not null)
        {
            await using var stream = req.File.OpenReadStream();

            var stored = await fileStore.SaveAsync(
                userId: DevUser.UserId,
                documentId: doc.Id,
                content: stream,
                originalFileName: req.File.FileName,
                contentType: req.File.ContentType,
                ct: ct);

            doc.AttachFile(stored.FileKey, stored.OriginalFileName, stored.ContentType, stored.SizeBytes, now);
            await db.SaveChangesAsync(ct);
        }
        
        var location = $"/documents/{doc.Id}";
        HttpContext.Response.Headers.Location = location;
        HttpContext.Response.StatusCode = StatusCodes.Status201Created;

        await HttpContext.Response.WriteAsJsonAsync(
            new CreateDocumentResponse (doc.Id),
            cancellationToken: ct);
    }
}

public sealed class CreateDocumentRequest
{
    public string Title { get; init; } = null!;
    public string Type { get; init; } = null!;
    public DateOnly? ExpiresOn { get; init; }
    public IFormFile? File { get; init; } = null!;
}

public sealed class CreateDocumentValidator : AbstractValidator<CreateDocumentRequest>
{
    public CreateDocumentValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);

        When(x => x.File is not null, () =>
        {
            RuleFor(x => x.File!.Length)
                .GreaterThan(0)
                .WithMessage("File is empty.");

            RuleFor(x => x.File!.Length)
                .LessThanOrEqualTo(10 * 1024 * 1024) // 10MB MVP cap
                .WithMessage("File is too large (max 10MB).");
        });
    }}
