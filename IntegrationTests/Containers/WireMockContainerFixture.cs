using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
namespace IntegrationTests.Containers
{
    public sealed class WireMockContainerFixture : ContainerFixture<IContainer>
    {
        public int Port { get; private set; }
        public string BaseUrl => $"http://localhost:{Port}";

        public WireMockContainerFixture()
            : base(new ContainerBuilder("wiremock/wiremock:3.13.2")
                .WithPortBinding(8080, true)
                .WithBindMount(Path.Combine(Environment.CurrentDirectory, "Mocks"), "/home/wiremock")
                .WithWaitStrategy(Wait.ForUnixContainer()
                        .UntilHttpRequestIsSucceeded(r => r
                            .ForPort(8080)
                            .ForPath("/__admin/health")
                        )
                )
                .Build())
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            Port = Container.GetMappedPublicPort(8080);
        }
    }
}
