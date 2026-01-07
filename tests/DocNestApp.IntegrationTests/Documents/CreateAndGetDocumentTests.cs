using DocNestApp.Contracts.Documents;

namespace DocNestApp.IntegrationTests.Documents;

using System.Net;
using System.Net.Http.Json;
using Infra;
using FluentAssertions;
using Xunit;
using DocNestApp.Contracts.Documents;

public sealed class CreateAndGetDocumentTests(PostgresFixture postgres) : IntegrationTestBase(postgres)
{
    [Fact]
    public async Task Create_then_get_returns_document()
    {
        var expiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7).ToString("yyyy-MM-dd");
        
        var post = await Client.PostDocumentMultipartAsync(
            "/documents",
            title: "testdoc",
            type: "ID",
            expiresOn: expiresOn);

        post.StatusCode.Should().Be(HttpStatusCode.Created);

        post.Headers.Location.Should().NotBeNull();
        var location = post.Headers.Location!.ToString();
        location.Should().StartWith("/documents/");

        var created = await post.Content.ReadFromJsonAsync<CreateDocumentResponse>();
        created!.Id.Should().NotBeEmpty();

        var get = await Client.GetAsync(location);
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var res = await get.Content.ReadFromJsonAsync<GetDocumentResponse>();
        res.Should().NotBeNull();

        var doc = res!.Document;

        doc.Id.Should().Be(created.Id);
        doc.Title.Should().Be("testdoc");
        doc.Type.Should().Be("ID");
        doc.ExpiresOn.Should().Be(DateOnly.Parse(expiresOn));
        doc.FileKey.Should().BeNull();
    }

    private sealed class CreateResponse
    {
        public Guid Id { get; set; }
    }

    private sealed class GetResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? ExpiresOn { get; set; }
        public bool HasFile { get; set; }
    }
}