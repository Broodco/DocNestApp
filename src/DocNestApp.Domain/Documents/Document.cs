namespace DocNestApp.Domain.Documents;

public sealed class Document
{
    private Document() { }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid SubjectId { get; private set; }

    public string Title { get; private set; } = null!;

    public string Type { get; private set; } = null!;
    
    public string? FileKey { get; private set; }
    
    public string? OriginalFileName { get; private set; }
    
    public string? ContentType { get; private set; }
    
    public long? SizeBytes { get; private set; }

    public bool HasFile => FileKey is not null;
    
    public DateOnly? ExpiresOn { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Document Create(
        Guid userId,
        Guid subjectId,
        string title,
        string type,
        DateOnly? expiresOn,
        DateTime utcNow)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (subjectId == Guid.Empty)
            throw new ArgumentException("SubjectId is required.", nameof(subjectId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type is required.", nameof(type));

        if (expiresOn is not null && expiresOn < DateOnly.FromDateTime(utcNow))
            throw new ArgumentException("Expiration date cannot be in the past.", nameof(expiresOn));

        return new Document
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SubjectId = subjectId,
            Title = title.Trim(),
            Type = type.Trim(),
            ExpiresOn = expiresOn,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }

    public void UpdateMetadata(
        string title,
        string type,
        DateOnly? expiresOn,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type is required.", nameof(type));

        if (expiresOn is not null && expiresOn < DateOnly.FromDateTime(utcNow))
            throw new ArgumentException("Expiration date cannot be in the past.", nameof(expiresOn));

        Title = title.Trim();
        Type = type.Trim();
        ExpiresOn = expiresOn;
        UpdatedAt = utcNow;
    }
    
    public void AttachFile(string fileKey, string originalFileName, string contentType, long sizeBytes, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(fileKey))
            throw new ArgumentException("FileKey is required.", nameof(fileKey));

        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("OriginalFileName is required.", nameof(originalFileName));

        if (sizeBytes <= 0)
            throw new ArgumentException("SizeBytes must be > 0.", nameof(sizeBytes));

        FileKey = fileKey.Trim();
        OriginalFileName = originalFileName.Trim();
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType.Trim();
        SizeBytes = sizeBytes;
        UpdatedAt = utcNow;
    }
}
