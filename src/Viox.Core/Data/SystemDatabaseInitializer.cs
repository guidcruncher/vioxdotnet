namespace Viox.Core.Data;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;

public class SystemDatabaseInitializer : IHostedService
{
    private readonly DataOptions _options;

    public SystemDatabaseInitializer(IOptions<DataOptions> options)
    {
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var connection = new SqliteConnection(_options.SystemConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Version(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Version TEXT NOT NULL
            );
        ";
        command.ExecuteNonQuery();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

}
