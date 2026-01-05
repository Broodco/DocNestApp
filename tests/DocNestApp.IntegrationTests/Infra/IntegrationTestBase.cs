namespace DocNestApp.IntegrationTests.Infra;

using Xunit;

[Collection("integration")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly PostgresFixture Postgres;
    protected readonly DocNestApiFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(PostgresFixture postgres)
    {
        Postgres = postgres;
        Factory = new DocNestApiFactory(Postgres.ConnectionString);
        Client = Factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await Postgres.ResetAsync();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        await Task.CompletedTask;
    }
}