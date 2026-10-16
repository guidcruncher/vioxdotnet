namespace Viox.Core.Data;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;

public class StatsDatabaseInitializer
{
    private readonly DataOptions _options;

    public StatsDatabaseInitializer(IOptions<DataOptions> options)
    {
        _options = options.Value;
    }

    public void Initialize()
    {
        using var connection = new SqliteConnection(_options.StatsConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS History(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Uri TEXT NOT NULL DEFAULT '',
                Metadata TEXT NOT NULL DEFAULT '',
                DurationSeconds INTEGER NOT NULL DEFAULT 0,
                CreatedTimestamp INTEGER NOT NULL
            );
        ";
        command.ExecuteNonQuery();
    }
}
