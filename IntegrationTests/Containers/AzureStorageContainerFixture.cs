using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;
using Testcontainers.Azurite;
using Testcontainers.MsSql;
namespace IntegrationTests.Containers
{
    public sealed class AzureStorageContainerFixture : ContainerFixture<IContainer>
    {
        public override int Port { get; protected set; }
        public string ConnectionString => $"";
        private static ushort AzuritePort => 10000;

        public AzureStorageContainerFixture()
            : base(new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.23.0")
                  .Build())
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            Port = Container.GetMappedPublicPort(AzuritePort);
        }
    }
}
