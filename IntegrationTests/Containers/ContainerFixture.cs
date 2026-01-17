using DotNet.Testcontainers.Containers;

namespace IntegrationTests.Containers
{
    public abstract class ContainerFixture<TContainer> : IAsyncLifetime
        where TContainer : IContainer
    {
        protected TContainer Container { get; }

        protected ContainerFixture(TContainer container)
        {
            Container = container;
        }

        public virtual async Task InitializeAsync()
        {
            await Container.StartAsync();
        }

        public virtual async Task DisposeAsync()
        {
            await Container.DisposeAsync();
        }
    }
}
