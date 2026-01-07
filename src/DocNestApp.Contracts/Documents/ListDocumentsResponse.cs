namespace DocNestApp.Contracts.Documents;

public sealed record ListDocumentsResponse(
    int Page,
    int PageSize,
    int Total,
    IReadOnlyList<DocumentDto> Items);