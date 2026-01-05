namespace DocNestApp.IntegrationTests.Documents;

using System.Net;
using System.Net.Http.Json;
using Infra;
using FluentAssertions;
using Xunit;

public sealed class CreateAndGetDocumentTests(PostgresFixture postgres) : IntegrationTestBase(postgres)
{
    [Fact]
    public async Task Create_then_get_returns_document()
    {
        var create = new
        {
            title = "testdoc",
            type = "ID",
            expiresOn = "2026-01-06"
        };

        var post = await Client.PostDocumentMultipartAsync(
            "/documents",
            title: "testdoc",
            type: "ID",
            expiresOn: "2026-01-06");

        post.StatusCode.Should().Be(HttpStatusCode.Created);

        post.Headers.Location.Should().NotBeNull();
        var location = post.Headers.Location!.ToString();
        location.Should().StartWith("/documents/");

        var created = await post.Content.ReadFromJsonAsync<CreateResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();

        var get = await Client.GetAsync(location);
        get.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = await get.Content.ReadFromJsonAsync<GetResponse>();
        doc.Should().NotBeNull();
        doc!.Id.Should().Be(created.Id);
        doc.Title.Should().Be("testdoc");
        doc.Type.Should().Be("ID");
        doc.ExpiresOn.Should().Be("2026-01-06");
        doc.HasFile.Should().BeFalse();
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