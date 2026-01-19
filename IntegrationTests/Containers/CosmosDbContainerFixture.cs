using Testcontainers.CosmosDb;

namespace IntegrationTests.Containers
{
    public sealed class CosmosDbContainerFixture : ContainerFixture<CosmosDbContainer>
    {
        public override int[] Ports { get; protected set; } 
        public HttpClient HttpClient => Container.HttpClient;
        public string ConnectionString { get; private set; }
        private static ushort CosmosPort => 8081;

        public CosmosDbContainerFixture()
            : base(new CosmosDbBuilder("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest")
                  .WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", "3")
                  .Build())
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            ConnectionString = Container.GetConnectionString();
            Ports = [.. new int[] {CosmosPort}.Select(x => (int)Container.GetMappedPublicPort(x))];
        }
    }
}
