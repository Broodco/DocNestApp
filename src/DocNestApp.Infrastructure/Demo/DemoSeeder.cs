using System.Text;
using DocNestApp.Application.Abstractions.Storage;
using DocNestApp.Domain.Documents;
using DocNestApp.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocNestApp.Infrastructure.Demo;

public sealed class DemoSeeder(
    AppDbContext db,
    IFileStore fileStore,
    IConfiguration config,
    ILogger<DemoSeeder> logger)
{
    public async Task SeedIfNeededAsync(DateTime utcNow, CancellationToken ct = default)
    {
        var demoMode = config.GetValue<bool>("DocNest:DemoMode");
        if (!demoMode)
            return;

        if (await db.Documents.AnyAsync(ct))
        {
            logger.LogInformation("Demo seeding skipped: documents already exist.");
            return;
        }

        var userId = config.GetValue<Guid>("DocNest:DemoUserId");
        var subjectId = config.GetValue<Guid>("DocNest:DemoSubjectId");
        if (userId == Guid.Empty || subjectId == Guid.Empty)
            throw new InvalidOperationException("DemoUserId and DemoSubjectId must be configured (non-empty GUIDs).");

        var today = DateOnly.FromDateTime(utcNow);

        // Create documents first (in memory)
        var docs = new List<Document>
        {
            Document.Create(userId, subjectId, "Car insurance policy", "Insurance", today.AddDays(14), utcNow),
            Document.Create(userId, subjectId, "ID card renewal", "Identity", today.AddDays(30), utcNow),
            Document.Create(userId, subjectId, "Gym subscription contract", "Contract", today.AddDays(7), utcNow),

            Document.Create(userId, subjectId, "Birth certificate", "CivilStatus", null, utcNow),
            Document.Create(userId, subjectId, "Diploma - Bachelor", "Education", null, utcNow),

            Document.Create(userId, subjectId, "Passport", "Identity", today.AddDays(365 * 4), utcNow),
            Document.Create(userId, subjectId, "Home lease", "Contract", today.AddDays(365), utcNow),

            Document.Create(userId, subjectId, "Electricity provider contract", "Utility", today.AddDays(90), utcNow),
            Document.Create(userId, subjectId, "Mutualité affiliation", "Health", null, utcNow),
            Document.Create(userId, subjectId, "Car registration", "Vehicle", today.AddDays(365 * 2), utcNow),
            Document.Create(userId, subjectId, "Internet subscription", "Utility", today.AddDays(60), utcNow),
            Document.Create(userId, subjectId, "Work contract", "Contract", null, utcNow),
            Document.Create(userId, subjectId, "Tax return 2025", "Tax", today.AddDays(120), utcNow),
            Document.Create(userId, subjectId, "Bank account agreement", "Bank", null, utcNow),
            Document.Create(userId, subjectId, "Warranty - Laptop", "Warranty", today.AddDays(365), utcNow)
        };

        // Attach 2 real files through IFileStore (so layout + fileKey match production behavior)
        await AttachDemoTextFileAsync(
            doc: docs.Single(d => d.Title == "Passport"),
            userId: userId,
            utcNow: utcNow,
            originalFileName: "passport-scan.txt",
            contentType: "text/plain",
            text: "Demo file - Passport scan placeholder.\nUploaded via demo seed.\n",
            ct: ct);

        await AttachDemoTextFileAsync(
            doc: docs.Single(d => d.Title == "Car insurance policy"),
            userId: userId,
            utcNow: utcNow,
            originalFileName: "insurance-policy.txt",
            contentType: "text/plain",
            text: "Demo file - Insurance policy placeholder.\nExpires soon.\n",
            ct: ct);

        db.Documents.AddRange(docs);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Demo seeding complete: inserted {Count} documents (with 2 attached files).", docs.Count);
    }

    private async Task AttachDemoTextFileAsync(
        Document doc,
        Guid userId,
        DateTime utcNow,
        string originalFileName,
        string contentType,
        string text,
        CancellationToken ct)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        await using var ms = new MemoryStream(bytes);

        var stored = await fileStore.SaveAsync(
            userId: userId,
            documentId: doc.Id,
            content: ms,
            originalFileName: originalFileName,
            contentType: contentType,
            ct: ct);

        doc.AttachFile(
            fileKey: stored.FileKey,
            originalFileName: stored.OriginalFileName,
            contentType: stored.ContentType,
            sizeBytes: stored.SizeBytes,
            utcNow: utcNow);
    }
}
