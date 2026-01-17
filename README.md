# .NET Integration Tests with WireMock and Testcontainers

A sample repository demonstrating how to set up integration tests for .NET 8 applications using WireMock and Testcontainers.

## Overview

This project showcases a modern approach to integration testing in .NET by combining:

- **WireMock** - For mocking external HTTP dependencies
- **Testcontainers** - For running WireMock and MS SQL Server in Docker containers during tests
- **xUnit** - As the testing framework
- **WebApplicationFactory** - For testing ASP.NET Core applications

## Project Structure

```
├── HttpTest/                          # Sample ASP.NET Core Web API
│   ├── Controllers/
│   │   └── JsonController.cs         # API controller that calls external service
│   ├── JsonService.cs                # HTTP client service
│   └── Program.cs                    # Application entry point
│
└── IntegrationTests/                 # Integration test project
    ├── Containers/
    │   ├── ContainerFixture.cs       # Base container fixture
    │   ├── WireMockContainerFixture.cs # WireMock container setup
    │   └── MsSqlContainerFixture.cs  # MS SQL Server container setup
    ├── Mocks/
    │   ├── mappings/                 # WireMock request/response mappings
    │   └── __files/                  # WireMock response files
    ├── TestFixture.cs                # Test application factory
    ├── TestCollection.cs             # xUnit collection definition
    └── TestCases.cs                  # Sample test cases
```

## Key Features

### WireMock Container Setup
- Uses WireMock Docker image (v3.13.2)
- Automatically binds to available port
- Mounts local mock definitions from `Mocks/` directory
- Includes health check wait strategy

### MS SQL Server Container Setup
- Uses MS SQL Server 2025 Docker image
- Automatically binds to available port
- Includes database availability wait strategy
- Exposes connection string for test use

### Test Fixture Architecture
- `ContainerFixture<TContainer>` - Generic base class for managing container lifecycle
- `WireMockContainerFixture` - Configures and manages WireMock container
- `MsSqlContainerFixture` - Configures and manages MS SQL Server container
- `TestFixture` - Configures the System Under Test (SUT) with mocked dependencies

### Integration with ASP.NET Core
- Uses `WebApplicationFactory<Program>` for in-memory test server
- Configures application to use integration test environment
- Overrides configuration to point to WireMock container

## Prerequisites

- .NET 8.0 SDK or later
- Docker Desktop (for running Testcontainers)
- Visual Studio 2022 or compatible IDE

## Getting Started

### 1. Clone the Repository

```sh
git clone https://github.com/hlim29/IntegrationTestsExample
cd IntegrationTests
```

### 2. Restore Dependencies

```sh
dotnet restore
```

### 3. Run the Tests

```sh
dotnet test
```

## Key Dependencies

```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.23" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.4" />
<PackageReference Include="Testcontainers.MsSql" Version="4.10.0" />
<PackageReference Include="WireMock.Net" Version="1.23.0" />
<PackageReference Include="WireMock.Net.Testcontainers" Version="1.23.0" />
<PackageReference Include="xunit" Version="2.5.3" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />
```

## Creating Mock Definitions

Place WireMock mappings in `IntegrationTests/Mocks/mappings/` and response files in `IntegrationTests/Mocks/__files/`. These files are automatically copied to the output directory and mounted into the WireMock container.

Example mapping structure:
```json
{
  "request": {
    "method": "GET",
    "url": "/httpbin/json"
  },
  "response": {
    "status": 200,
    "bodyFileName": "sample.json"
  }
}
```

## Writing Tests

Tests use xUnit's `ICollectionFixture` to share the test fixtures across test classes:

```csharp
[Collection("SUT")]
public class TestCases
{
    private readonly HttpClient _client;

    public TestCases(TestFixture testFixture)
    {
        _client = testFixture.CreateSutClient();
    }

    [Fact]
    public async Task Success()
    {
        var res = await _client.GetAsync("json");
        res.EnsureSuccessStatusCode();
        Assert.True(res.StatusCode == System.Net.HttpStatusCode.OK);
    }
}
```

## Configuration

The test fixture dynamically configures the application to use the WireMock container:

```csharp
builder.ConfigureAppConfiguration((_, config) =>
{
    config.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["httpbin:baseAddress"] = $"{_wireMock.BaseUrl}/httpbin/"
    });
});
```

The MS SQL Server container provides a connection string that can be used in your tests:

```csharp
string connectionString = _msSql.ConnectionString;
// Use this connection string to configure your database context or services
```

## How It Works

1. **Container Initialization**: When tests start, both the `WireMockContainerFixture` and `MsSqlContainerFixture` create and start their respective containers
2. **Port Mapping**: Each container's internal port is mapped to a random available host port
3. **Configuration Override**: The `TestFixture` injects the WireMock URL into the application configuration
4. **Test Execution**: Tests interact with the ASP.NET Core application, which makes HTTP calls to WireMock and can use the SQL Server database
5. **Cleanup**: After tests complete, all containers are automatically stopped and disposed

## Benefits of This Approach

- ✅  **Isolated Tests** - Each test run uses a fresh container instance
- ✅ **Repeatable** - Tests produce consistent results across environments
- ✅ **Fast** - Containers start quickly and run in parallel
- ✅ **Realistic** - Tests against actual HTTP endpoints, not mocked interfaces
- ✅ **Portable** - Works on any machine with Docker installed
- ✅ **No Manual Setup** - No need to manually start/stop external services

## Architecture Highlights

### Container Lifecycle Management
The `ContainerFixture<TContainer>` base class implements `IAsyncLifetime` from xUnit, ensuring proper initialization and cleanup:

```csharp
public abstract class ContainerFixture<TContainer> : IAsyncLifetime
    where TContainer : IContainer
{
    protected TContainer Container { get; }

    public virtual async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}
```

### Test Collection Sharing
The `TestCollection` class uses xUnit's collection fixtures to share containers across multiple test classes, improving performance:

```csharp
[CollectionDefinition("SUT")]
public class TestCollection : ICollectionFixture<WireMockContainerFixture>, ICollectionFixture<MsSqlContainerFixture>, ICollectionFixture<TestFixture>
{
}
```

## Extending This Pattern

You can easily extend this pattern to add more containers (databases, message queues, , etc.):

1. Create a new fixture class inheriting from `ContainerFixture<TContainer>`
2. Add it to the `TestCollection` as another `ICollectionFixture<T>`
3. Inject it into your `TestFixture` to configure the application

Example of the MS SQL Server container fixture:

```csharp
public sealed class MsSqlContainerFixture : ContainerFixture<IContainer>
{
    public override int Port { get; protected set; }
    
    public string ConnectionString => 
        $"server=localhost,{Port};user id={MsSqlBuilder.DefaultUsername};password={MsSqlBuilder.DefaultPassword};database={MsSqlBuilder.DefaultDatabase}";

    public MsSqlContainerFixture()
        : base(new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
              .WithWaitStrategy(Wait.ForUnixContainer().UntilDatabaseIsAvailable(SqlClientFactory.Instance))
              .Build())
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        Port = Container.GetMappedPublicPort(1433);
    }
}
```

## Troubleshooting

### Docker Issues
- Ensure Docker Desktop is running
- Check that your user has permissions to access Docker

### Port Conflicts
- The framework automatically assigns available ports, so conflicts should be rare
- Check that no other services are blocking the port range

### Container Startup Timeout
- WireMock container includes a health check that waits for the admin API to be available
- MS SQL Server container includes a wait strategy that ensures the database is ready before tests run
- If tests fail due to timeout, check Docker logs for the respective containers

## License

This is a sample project for demonstration purposes.
