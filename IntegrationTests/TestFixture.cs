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

        public TestFixture()
        {
            _wireMock = new WireMockContainerFixture();
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
                    ["httpbin:baseAddress"] = $"http://localhost:{_wireMock.Port}/httpbin/"
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
            await _wireMock.InitializeAsync();
        }

        public async Task DisposeAsync()
        {
            await _wireMock.DisposeAsync();
        }
    }
}
