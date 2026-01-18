# .NET Integration Tests with WireMock and Testcontainers

A sample repository demonstrating how to set up integration tests for .NET 8 applications using WireMock, Testcontainers, and Azure Storage emulation.

## Overview

This project showcases a modern approach to integration testing in .NET by combining:

- **WireMock** - For mocking external HTTP dependencies
- **Testcontainers** - For running WireMock, MS SQL Server, and Azurite in Docker containers during tests
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
│   ├── MsSqlContainerFixture.cs  # MS SQL Server container setup
│   └── AzureStorageContainerFixture.cs # Azure Storage (Azurite) container setup
├── Mocks/
│   ├── mappings/                 # WireMock request/response mappings
│   └── __files/                  # WireMock response files
├── Sql/
│   └── CreateUsers.sql           # Sample SQL scripts for testing
├── TestFixture.cs                # Test application factory
├── TestCollection.cs             # xUnit collection definition
├── TestCases.cs                  # Sample test cases
└── appsettings.IntegrationTests.json # Integration test configuration
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
- Helper methods to execute SQL scripts and commands

### Azure Storage Container Setup
- Uses Azurite Docker image (v3.35.0) for Azure Storage emulation
- Emulates Blob, Queue, and Table storage
- Automatically binds to available ports (10000-10002)
- Provides BlobServiceClient for easy interaction
- Includes helper methods to create blob containers

### Test Fixture Architecture
- `ContainerFixture<TContainer>` - Generic base class for managing container lifecycle
- `WireMockContainerFixture` - Configures and manages WireMock container
- `MsSqlContainerFixture` - Configures and manages MS SQL Server container
- `AzureStorageContainerFixture` - Configures and manages Azurite container
- `TestFixture` - Configures the System Under Test (SUT) with mocked dependencies

### Integration with ASP.NET Core
- Uses `WebApplicationFactory<Program>` for in-memory test server
- Configures application to use integration test environment
- Loads custom configuration from `appsettings.IntegrationTests.json`
- Overrides configuration to point to WireMock container

## Prerequisites

- .NET 8.0 SDK or later
- Docker Desktop (for running Testcontainers)
- Visual Studio 2022 or compatible IDE

## Getting Started

### 1. Clone the Repository

```
git clone https://github.com/hlim29/IntegrationTestsExample
cd IntegrationTests
```

### 2. Restore Dependencies

```
dotnet restore
```

### 3. Run the Tests

```
dotnet test
```

## Key Dependencies

```
<PackageReference Include="Azure.Storage.Blobs" Version="12.24.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.23" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.4" />
<PackageReference Include="Testcontainers.Azurite" Version="4.10.0" />
<PackageReference Include="Testcontainers.MsSql" Version="4.10.0" />
<PackageReference Include="WireMock.Net" Version="1.23.0" />
<PackageReference Include="WireMock.Net.Testcontainers" Version="1.23.0" />
<PackageReference Include="xunit" Version="2.5.3" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />
```

## Creating Mock Definitions

Place WireMock mappings in `IntegrationTests/Mocks/mappings/` and response files in `IntegrationTests/Mocks/__files/`. These files are automatically copied to the output directory and mounted into the WireMock container.

Example mapping structure:
```
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

```
[Collection("SUT")]
public class TestCases : IClassFixture<TestFixture>
{
    private readonly TestFixture _testFixture;
    private readonly HttpClient _client;

    public TestCases(TestFixture testFixture)
    {
        _testFixture = testFixture;
        _client = testFixture.CreateSutClient();
    }

    [Fact]
    public async Task Success()
    {
        var res = await _client.GetAsync("json");
        res.EnsureSuccessStatusCode();
        Assert.True(res.StatusCode == System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task BlobStorage_tests()
    {
        var blobService = _testFixture.AzureStorage.BlobServiceClient;
        var container = await blobService.CreateBlobContainerAsync("teststorage");

        var blob = container.Value.GetBlobClient("test.txt");
        await blob.UploadAsync(BinaryData.FromString("Hello from .NET"), overwrite: true);

        Assert.True(await blob.ExistsAsync());
    }

    [Fact]
    public async Task MsSql_tests()
    {
        var sql = _testFixture.MsSql;
        await sql.ExecuteSqlFileAsync("Sql/CreateUsers.sql");

        await using var connection = new SqlConnection(sql.ConnectionString);
        await connection.OpenAsync();

        await using var countCommand = new SqlCommand("SELECT COUNT(*) FROM dbo.Users", connection);
        var rowCount = (int)await countCommand.ExecuteScalarAsync();
        Assert.Equal(5, rowCount);
    }
}
```

## Configuration

The test fixture dynamically configures the application to use the test containers:

```
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

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile(
                Path.Combine(AppContext.BaseDirectory, "appsettings.IntegrationTests.json"),
                optional: false,
                reloadOnChange: false);

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["httpbin:baseAddress"] = $"{WireMock.BaseUrl}/httpbin/"
            });
        });
    }
}
```

### Working with MS SQL Server

The `MsSqlContainerFixture` provides helper methods for executing SQL:

```
// Execute SQL from a string
await _msSql.ExecuteSqlAsync("CREATE TABLE Users (Id INT, Name NVARCHAR(100))");

// Execute SQL from a file
await _msSql.ExecuteSqlFileAsync("Sql/CreateUsers.sql");

// Use the connection string directly
await using var connection = new SqlConnection(_msSql.ConnectionString);
await connection.OpenAsync();
```

### Working with Azure Storage

The `AzureStorageContainerFixture` provides access to Azure Storage services:

```
// Get the BlobServiceClient
var blobService = _testFixture.AzureStorage.BlobServiceClient;

// Create a blob container
var container = await blobService.CreateBlobContainerAsync("mycontainer");

// Upload a blob
var blob = container.Value.GetBlobClient("myfile.txt");
await blob.UploadAsync(BinaryData.FromString("Hello World"), overwrite: true);

// Use the connection string directly
string connectionString = _testFixture.AzureStorage.ConnectionString;
```

## How It Works

1. **Container Initialisation**: When tests start, all container fixtures (`WireMockContainerFixture`, `MsSqlContainerFixture`, and `AzureStorageContainerFixture`) create and start their respective containers
2. **Port Mapping**: Each container's internal port(s) are mapped to random available host ports
3. **Configuration Override**: The `TestFixture` injects container URLs and connection strings into the application configuration
4. **Test Execution**: Tests interact with the ASP.NET Core application, which makes HTTP calls to WireMock, connects to SQL Server, and uses Azure Storage
5. **Cleanup**: After tests complete, all containers are automatically stopped and disposed

## Benefits of This Approach

- ✅ **Isolated Tests** - Each test run uses fresh container instances
- ✅ **Repeatable** - Tests produce consistent results across environments
- ✅ **Fast** - Containers start quickly and run in parallel
- ✅ **Realistic** - Tests against actual services, not mocked interfaces
- ✅ **Portable** - Works on any machine with Docker installed
- ✅ **No Manual Setup** - No need to manually start/stop external services
- ✅ **Complete Coverage** - Test HTTP dependencies, databases, and cloud storage in one solution

## Architecture Highlights

### Container Lifecycle Management
The `ContainerFixture<TContainer>` base class implements `IAsyncLifetime` from xUnit, ensuring proper initialization and cleanup:

```
public abstract class ContainerFixture<TContainer> : IAsyncLifetime
    where TContainer : IContainer
{
    protected TContainer Container { get; }
    public abstract int[] Ports { get; protected set; }

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

```
[CollectionDefinition("SUT")]
public class TestCollection : ICollectionFixture<WireMockContainerFixture>, 
    ICollectionFixture<MsSqlContainerFixture>, 
    ICollectionFixture<AzureStorageContainerFixture>
{
}
```

## Extending This Pattern

You can easily extend this pattern to add more containers (Redis, RabbitMQ, MongoDB, etc.):

1. Create a new fixture class inheriting from `ContainerFixture<TContainer>`
2. Add it to the `TestCollection` as another `ICollectionFixture<T>`
3. Inject it into your `TestFixture` to configure the application

Example of the Azure Storage container fixture:

```
public sealed class AzureStorageContainerFixture : ContainerFixture<IContainer>
{
    public override int[] Ports { get; protected set; }
    public BlobServiceClient BlobServiceClient => new BlobServiceClient(ConnectionString);
    private static int[] AzuritePorts => [10000, 10001, 10002];
    private const string AccountName = "devstoreaccount1";
    private const string AccountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

    public string ConnectionString => $"DefaultEndpointsProtocol=http;AccountName={AccountName};AccountKey={AccountKey};BlobEndpoint=http://127.0.0.1:{Ports[0]}/{AccountName};QueueEndpoint=http://127.0.0.1:{Ports[1]}/{AccountName};TableEndpoint=http://127.0.0.1:{Ports[2]}/{AccountName};";

    public AzureStorageContainerFixture()
        : base(new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.35.0")
              .WithCommand("azurite", "--oauth", "basic", "--skipApiVersionCheck")
              .WithEnvironment("AZURITE_ACCOUNTS", $"{AccountName}:{AccountKey}")
              .Build())
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        Ports = [.. AzuritePorts.Select(x => (int)Container.GetMappedPublicPort(x))];
    }
}
```

Example of the MS SQL Server container fixture with helper methods:

```
public sealed class MsSqlContainerFixture : ContainerFixture<IContainer>
{
    public override int[] Ports { get; protected set; }
    
    public string ConnectionString => 
        $"server=localhost,{Ports.First()};user id={MsSqlBuilder.DefaultUsername};password={MsSqlBuilder.DefaultPassword};database={MsSqlBuilder.DefaultDatabase};TrustServerCertificate=true";

    public MsSqlContainerFixture()
        : base(new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
              .WithWaitStrategy(Wait.ForUnixContainer().UntilDatabaseIsAvailable(SqlClientFactory.Instance))
              .Build())
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        Ports = [.. new int[] { 1433 }.Select(x => (int)Container.GetMappedPublicPort(x))];
    }

    public async Task ExecuteSqlAsync(string sql)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task ExecuteSqlFileAsync(string filePath)
    {
        var sql = await File.ReadAllTextAsync(filePath);
        await ExecuteSqlAsync(sql);
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
- Azurite container starts quickly and is ready immediately
- If tests fail due to timeout, check Docker logs for the respective containers

## License

This is a sample project for demonstration purposes.
