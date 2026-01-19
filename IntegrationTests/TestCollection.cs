namespace IntegrationTests
{
    using IntegrationTests.Containers;
    using Xunit;

    [CollectionDefinition("SUT")]
    public class TestCollection : ICollectionFixture<WireMockContainerFixture>, ICollectionFixture<MsSqlContainerFixture>, ICollectionFixture<AzureStorageContainerFixture>, ICollectionFixture<CosmosDbContainerFixture>
    {
    }
}
