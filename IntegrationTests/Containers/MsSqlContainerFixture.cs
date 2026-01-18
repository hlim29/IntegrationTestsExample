using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
namespace IntegrationTests.Containers
{
    public sealed class MsSqlContainerFixture : ContainerFixture<IContainer>
    {
        public override int[] Ports { get; protected set; }
        public string ConnectionString => $"server=localhost,{Ports.First()};user id={MsSqlBuilder.DefaultUsername};password={MsSqlBuilder.DefaultPassword};database={MsSqlBuilder.DefaultDatabase};TrustServerCertificate=true";
        private static ushort MsSqlPort => 1433;

        public MsSqlContainerFixture()
            : base(new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
                  .WithWaitStrategy(Wait.ForUnixContainer().UntilDatabaseIsAvailable(SqlClientFactory.Instance))
                  .Build())
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            Ports = [.. new int[] { MsSqlPort }.Select(x => (int)Container.GetMappedPublicPort(x))];
        }

        public async Task ExecuteSqlAsync(string sql)
        {
            await using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();
            await using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Executes SQL from a file against the MS SQL container.
        /// await _msSql.ExecuteSqlFileAsync("Scripts/CreateTables.sql");
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task ExecuteSqlFileAsync(string filePath)
        {
            var sql = await File.ReadAllTextAsync(filePath);
            await ExecuteSqlAsync(sql);
        }
    }
}
