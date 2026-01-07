namespace DocNestApp.Contracts.Documents;

public sealed record DocumentDto(
    Guid Id,
    Guid UserId,
    Guid SubjectId,
    string Title,
    string Type,
    DateOnly? ExpiresOn,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? FileKey,
    string? OriginalFileName,
    string? ContentType,
    long? SizeBytes);