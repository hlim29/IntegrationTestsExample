using IntegrationTests.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Data;
using System.Data.Common;

namespace IntegrationTests
{
    public sealed class TestFixture : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly WireMockContainerFixture _wireMock;
        private readonly MsSqlContainerFixture _msSql;

        public TestFixture()
        {
            _wireMock = new WireMockContainerFixture();
            _msSql = new MsSqlContainerFixture();
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

        public async Task InitializeAsync()
        {
            await _msSql.InitializeAsync();
            await _wireMock.InitializeAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _msSql.DisposeAsync();
            await _wireMock.DisposeAsync();
        }
    }
}
