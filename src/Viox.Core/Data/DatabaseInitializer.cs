namespace Viox.Core.Data;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;

public class DatabaseInitializer
{
    private readonly DataOptions _options;

    public DatabaseInitializer(IOptions<DataOptions> options)
    {
        _options = options.Value;
    }

    public void Initialize()
    {
        using var connection = new SqliteConnection(_options.SqliteConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS History(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Uri TEXT NOT NULL,
                Metadata TEXT,
                CreatedTimestamp INTEGER NOT NULL
            );
        ";
        command.ExecuteNonQuery();
    }
}
