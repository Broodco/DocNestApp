using FluentValidation;

namespace DocNestApp.Api.Features.Documents.List;

using FastEndpoints;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

public sealed class ListDocumentsEndpoint(AppDbContext db)
    : Endpoint<ListDocumentsRequest, ListDocumentsResponse>
{
    public override void Configure()
    {
        Get("/documents");
        AllowAnonymous(); // MVP
        Validator<ListDocumentsValidator>();
        
        Summary(s =>
        {
            s.Summary = "List documents";
            s.Description = "Returns a paged list of documents for the current user (dev user for now). Supports light filtering.";
        });
    }

    public override async Task HandleAsync(ListDocumentsRequest req, CancellationToken ct)
    {
        var userId = DevUser.UserId;

        var page = req.Page <= 0 ? 1 : req.Page;
        var pageSize = req.PageSize <= 0 ? 20 : req.PageSize;
        pageSize = Math.Min(pageSize, 50);

        var query = db.Documents
            .AsNoTracking()
            .Where(d => d.UserId == userId);

        // q (title search)
        if (!string.IsNullOrWhiteSpace(req.Q))
        {
            var q = req.Q.Trim();

            // Postgres: ILIKE for case-insensitive contains
            query = query.Where(d => EF.Functions.ILike(d.Title, $"%{q}%"));
        }

        if (!string.IsNullOrWhiteSpace(req.Type))
        {
            var type = req.Type.Trim();

            query = query.Where(d => d.Type.ToLower() == type.ToLower());
        }

        var hasExpiryFilter = req.ExpiresBefore is not null || req.ExpiresAfter is not null;

        if (hasExpiryFilter)
        {
            if (req.IncludeNoExpiry)
            {
                query = query.Where(d =>
                    d.ExpiresOn == null ||
                    (req.ExpiresBefore == null || d.ExpiresOn <= req.ExpiresBefore) &&
                    (req.ExpiresAfter == null || d.ExpiresOn >= req.ExpiresAfter));
            }
            else
            {
                // only docs with a date
                query = query.Where(d => d.ExpiresOn != null);

                if (req.ExpiresBefore is not null)
                    query = query.Where(d => d.ExpiresOn <= req.ExpiresBefore);

                if (req.ExpiresAfter is not null)
                    query = query.Where(d => d.ExpiresOn >= req.ExpiresAfter);
            }
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .ThenByDescending(d => d.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DocumentListItem
            {
                Id = d.Id,
                Title = d.Title,
                Type = d.Type,
                HasFile = d.FileKey != null,
                ExpiresOn = d.ExpiresOn,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync(ct);

        var response = new ListDocumentsResponse
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        };

        await Send.OkAsync(response, ct);
    }
}

public sealed class ListDocumentsRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Q { get; init; }
    public string? Type { get; init; }
    public DateOnly? ExpiresBefore { get; init; }
    public DateOnly? ExpiresAfter { get; init; }
    public bool IncludeNoExpiry { get; init; } = false;
}

public sealed class ListDocumentsResponse
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public IReadOnlyList<DocumentListItem> Items { get; init; } = Array.Empty<DocumentListItem>();
}

public sealed class DocumentListItem
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string Type { get; init; } = null!;
    public DateOnly? ExpiresOn { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public bool HasFile { get; init; }
}

public sealed class ListDocumentsValidator : AbstractValidator<ListDocumentsRequest>
{
    public ListDocumentsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be >= 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("PageSize must be between 1 and 50.");

        RuleFor(x => x.Q)
            .MaximumLength(200)
            .WithMessage("Query is too long.")
            .When(x => !string.IsNullOrWhiteSpace(x.Q));

        RuleFor(x => x.Type)
            .MaximumLength(50)
            .WithMessage("Type is too long.")
            .When(x => !string.IsNullOrWhiteSpace(x.Type));

        // If both dates are provided, ensure the range makes sense
        RuleFor(x => x)
            .Must(x => x.ExpiresAfter is null || x.ExpiresBefore is null || x.ExpiresAfter <= x.ExpiresBefore)
            .WithMessage("ExpiresAfter must be <= ExpiresBefore.")
            .When(x => x.ExpiresAfter is not null && x.ExpiresBefore is not null);

        // If IncludeNoExpiry is true, at least one expiry filter must be set
        RuleFor(x => x.IncludeNoExpiry)
            .Must((req, include) => !include || req.ExpiresAfter is not null || req.ExpiresBefore is not null)
            .WithMessage("IncludeNoExpiry only makes sense when ExpiresAfter and/or ExpiresBefore is provided.");
    }
}