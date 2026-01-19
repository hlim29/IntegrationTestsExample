using Azure.Storage.Blobs;
using DotNet.Testcontainers.Containers;
using Testcontainers.Azurite;
namespace IntegrationTests.Containers
{
    public sealed class AzureStorageContainerFixture : ContainerFixture<AzuriteContainer>
    {
        public override int[] Ports { get; protected set; }
        public BlobServiceClient BlobServiceClient => new BlobServiceClient(ConnectionString);
        private static int[] AzuritePorts => [10000, 10001, 10002];
        private const string AccountName = "devstoreaccount1";
        private const string AccountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

        public string ConnectionString { get; private set; }

        public AzureStorageContainerFixture()
            : base(new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.35.0")
                  .WithCommand("azurite", "--oauth", "basic", "--skipApiVersionCheck")
                  .WithEnvironment("AZURITE_ACCOUNTS", $"{AccountName}:{AccountKey}")
                  .Build())
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            ConnectionString = Container.GetConnectionString();
            Ports = [.. AzuritePorts.Select(x => (int)Container.GetMappedPublicPort(x))];
        }

        public async Task CreateBlobStorage(string storageName)
        {
            var bsc = new BlobServiceClient(ConnectionString);
            await bsc.CreateBlobContainerAsync(storageName);
        }
    }
}
