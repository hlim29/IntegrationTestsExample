using IntegrationTests.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace IntegrationTests
{
    public sealed class TestFixture : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly WireMockContainerFixture _wireMock;
        private readonly MsSqlContainerFixture _msSql;
        private readonly AzureStorageContainerFixture _azureStorage;

        public TestFixture(
            WireMockContainerFixture wireMock,
            MsSqlContainerFixture msSql,
            AzureStorageContainerFixture azureStorage)
        {
            _wireMock = wireMock;
            _msSql = msSql;
            _azureStorage = azureStorage;
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("IntegrationTest");

            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["httpbin:baseAddress"] = $"{_wireMock.BaseUrl}/httpbin/"
                });
            });

            builder.ConfigureServices(services =>
            {
            });
        }

        public HttpClient CreateSutClient()
        {
            return CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
    }
}
