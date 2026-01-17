using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;
using Testcontainers.Azurite;
using Testcontainers.MsSql;
namespace IntegrationTests.Containers
{
    public sealed class AzureStorageContainerFixture : ContainerFixture<IContainer>
    {
        public override int[] Ports { get; protected set; }
        private static int[] AzuritePorts => [10000, 10001, 10002];

        public AzureStorageContainerFixture()
            : base(new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.23.0")
                  .Build())
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            Ports = [.. AzuritePorts.Select(x => (int)Container.GetMappedPublicPort(x))];
        }
    }
}
