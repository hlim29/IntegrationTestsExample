using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
namespace IntegrationTests.Containers
{
    public sealed class WireMockContainerFixture : ContainerFixture<IContainer>
    {
        private static ushort WireMockPort => 8080;
        public override int[] Ports { get; protected set; }
        public string BaseUrl => $"http://localhost:{Ports.First()}";

        public WireMockContainerFixture()
            : base(new ContainerBuilder("wiremock/wiremock:3.13.2")
                .WithPortBinding(WireMockPort, true)
                .WithBindMount(Path.Combine(Environment.CurrentDirectory, "Mocks"), "/home/wiremock")
                .WithWaitStrategy(Wait.ForUnixContainer()
                        .UntilHttpRequestIsSucceeded(r => r
                            .ForPort(WireMockPort)
                            .ForPath("/__admin/health")
                        )
                )
                .Build())
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            Ports = [.. new int[] { WireMockPort }.Select(x => (int)Container.GetMappedPublicPort(x))];
        }
    }
}
