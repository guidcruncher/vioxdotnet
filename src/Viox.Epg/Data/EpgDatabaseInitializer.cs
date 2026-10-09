namespace Viox.Epg.Data;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

using Viox.Epg.Configuration;

public class EpgDatabaseInitializer
{
    private readonly EpgOptions _options;

    public EpgDatabaseInitializer(IOptions<EpgOptions> options)
    {
        _options = options.Value;
    }

    public void Initialize()
    {
        using var connection = new SqliteConnection(_options.SqliteConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Channels (
                Id TEXT PRIMARY KEY,
                DisplayName TEXT NOT NULL,
                Url TEXT,
                IconUrl TEXT
            );

            CREATE TABLE IF NOT EXISTS Programmes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ChannelId TEXT NOT NULL,
                Start TEXT NOT NULL,
                Stop TEXT NOT NULL,
                Title TEXT,
                SubTitle TEXT,
                Description TEXT,
                EpisodeNum TEXT,
                IconUrl TEXT,
                FOREIGN KEY(ChannelId) REFERENCES Channels(Id)
            );

            CREATE TABLE IF NOT EXISTS ProgrammeCategories (
                ProgrammeId INTEGER NOT NULL,
                Category TEXT NOT NULL,
                FOREIGN KEY(ProgrammeId) REFERENCES Programmes(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS EpgMetadata (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            );
        ";
        command.ExecuteNonQuery();
    }
}
