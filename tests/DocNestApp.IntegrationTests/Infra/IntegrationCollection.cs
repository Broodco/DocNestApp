namespace DocNestApp.IntegrationTests.Infra;

using Xunit;

[CollectionDefinition("integration")]
public sealed class IntegrationCollection : ICollectionFixture<PostgresFixture> { }