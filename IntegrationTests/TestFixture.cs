using IntegrationTests.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace IntegrationTests
{
    public sealed class TestFixture : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public WireMockContainerFixture WireMock { get; private set; }
        public MsSqlContainerFixture MsSql { get; private set; }
        public AzureStorageContainerFixture AzureStorage { get; private set; }

        public TestFixture(
            WireMockContainerFixture wireMock,
            MsSqlContainerFixture msSql,
            AzureStorageContainerFixture azureStorage)
        {
            WireMock = wireMock;
            MsSql = msSql;
            AzureStorage = azureStorage;
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
                // Add the IntegrationTests appsettings file
                config.AddJsonFile(
                    Path.Combine(AppContext.BaseDirectory, "appsettings.IntegrationTests.json"),
                    optional: false,
                    reloadOnChange: false);

                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["httpbin:baseAddress"] = $"{WireMock.BaseUrl}/httpbin/"
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

