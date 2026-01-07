namespace DocNestApp.Contracts.Documents;

public sealed record ListDocumentsRequest(
    int Page = 1,
    int PageSize = 20,
    string? Q = null,
    string? Type = null,
    DateOnly? ExpiresAfter = null,
    DateOnly? ExpiresBefore = null,
    bool IncludeNoExpiry = false);