namespace DocNestApp.IntegrationTests.Documents;

using System.Net;
using System.Net.Http.Json;
using Infra;
using FluentAssertions;
using Xunit;

public sealed class ListDocumentsTests : IntegrationTestBase
{
    public ListDocumentsTests(PostgresFixture postgres) : base(postgres) { }

    [Fact]
    public async Task List_supports_light_filters()
    {
        await Create("Passport - Maxime", "ID", "2026-02-01");
        await Create("Netflix subscription", "SUBSCRIPTION", null);
        await Create("ID card - spouse", "ID", "2026-01-10");

        var res = await Client.GetAsync("/documents?type=ID&page=1&pageSize=50");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await res.Content.ReadFromJsonAsync<ListResponse>();
        list.Should().NotBeNull();
        list!.Total.Should().Be(2);
        list.Items.Should().OnlyContain(x => x.Type == "ID");

        var res2 = await Client.GetAsync("/documents?q=passport&page=1&pageSize=50");
        res2.StatusCode.Should().Be(HttpStatusCode.OK);

        var list2 = await res2.Content.ReadFromJsonAsync<ListResponse>();
        list2!.Total.Should().Be(1);
        list2.Items[0].Title.Should().Contain("Passport");
    }

    private async Task Create(string title, string type, string? expiresOn)
    {
        var payload = new CreateDocumentRequest
        {
            Title = title,
            Type = type,
            ExpiresOn = expiresOn
        };

        var post = await Client.PostDocumentMultipartAsync("/documents", title, type, expiresOn);
        post.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    
    private sealed class ListResponse
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<Item> Items { get; set; } = [];
    }

    private sealed class Item
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? ExpiresOn { get; set; }
        public bool HasFile { get; set; }
    }
    
    private sealed class CreateDocumentRequest
    {
        public string Title { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? ExpiresOn { get; set; }
    }
}
