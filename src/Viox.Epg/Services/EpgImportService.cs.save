namespace Viox.Epg.Services;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Epg.Configuration;
using Viox.Epg.Data;
using Viox.Epg.Models;

public class EpgImportService
{
    private readonly EpgDatabaseInitializer _dbInitializer;
    private readonly EpgDownloader _downloader;
    private readonly XmltvParser _parser;
    private readonly EpgOptions _options;
    private readonly ILogger<EpgImportService> _logger;

    private const string LastSuccessfulImportKey = "LastSuccessfulImportUtc";

    public EpgImportService(
        EpgDatabaseInitializer dbInitializer,
        EpgDownloader downloader,
        XmltvParser parser,
        IOptions<EpgOptions> options,
        ILogger<EpgImportService> logger)
    {
        _dbInitializer = dbInitializer;
        _downloader = downloader;
        _parser = parser;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> HasRunTodayAsync(CancellationToken cancellationToken = default)
    {
        _dbInitializer.Initialize();

        using var connection = new SqliteConnection(_options.SqliteConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Value FROM EpgMetadata WHERE Key = @Key;";
        command.Parameters.AddWithValue("@Key", LastSuccessfulImportKey);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is string valueStr && DateTime.TryParse(valueStr, out var lastRunUtc))
        {
            return lastRunUtc.Date == DateTime.UtcNow.Date;
        }

        return false;
    }

    public async Task ImportAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing EPG database...");
        _dbInitializer.Initialize();

        _logger.LogInformation("Downloading EPG data from {SourceUrl}...", _options.SourceUrl);
        await using var stream = await _downloader.DownloadAndDecompressAsync(cancellationToken);

        using var connection = new SqliteConnection(_options.SqliteConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = connection.BeginTransaction();

        try
        {
            _logger.LogInformation("Parsing and importing XMLTV data...");

            await _parser.ParseAsync(
                stream,
                async channel => await InsertChannelAsync(connection, transaction, channel),
                async programme => await InsertProgrammeAsync(connection, transaction, programme),
                cancellationToken);

            await SaveMetadataAsync(connection, transaction, LastSuccessfulImportKey, DateTime.UtcNow.ToString("O"));

            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("EPG import completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import EPG data. Rolling back transaction.");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task SaveMetadataAsync(SqliteConnection connection, SqliteTransaction transaction, string key, string value)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            INSERT INTO EpgMetadata (Key, Value)
            VALUES (@Key, @Value)
            ON CONFLICT(Key) DO UPDATE SET
                Value = excluded.Value;
        ";
        command.Parameters.AddWithValue("@Key", key);
        command.Parameters.AddWithValue("@Value", value);

        await command.ExecuteNonQueryAsync();
    }

    private static async Task InsertChannelAsync(SqliteConnection connection, SqliteTransaction transaction, ChannelRecord channel)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            INSERT INTO Channels (Id, DisplayName, Url, IconUrl)
            VALUES (@Id, @DisplayName, @Url, @IconUrl)
            ON CONFLICT(Id) DO UPDATE SET
                DisplayName = excluded.DisplayName,
                Url = excluded.Url,
                IconUrl = excluded.IconUrl;
        ";
        command.Parameters.AddWithValue("@Id", channel.Id);
        command.Parameters.AddWithValue("@DisplayName", channel.DisplayName);
        command.Parameters.AddWithValue("@Url", (object?)channel.Url ?? DBNull.Value);
        command.Parameters.AddWithValue("@IconUrl", (object?)channel.IconUrl ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    private static async Task InsertProgrammeAsync(SqliteConnection connection, SqliteTransaction transaction, ProgrammeRecord programme)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            INSERT INTO Programmes (ChannelId, Start, Stop, Title, SubTitle, Description, EpisodeNum, IconUrl)
            VALUES (@ChannelId, @Start, @Stop, @Title, @SubTitle, @Description, @EpisodeNum, @IconUrl);
            SELECT last_insert_rowid();
        ";
        command.Parameters.AddWithValue("@ChannelId", programme.ChannelId);
        command.Parameters.AddWithValue("@Start", programme.Start);
        command.Parameters.AddWithValue("@Stop", programme.Stop);
        command.Parameters.AddWithValue("@Title", (object?)programme.Title ?? DBNull.Value);
        command.Parameters.AddWithValue("@SubTitle", (object?)programme.SubTitle ?? DBNull.Value);
        command.Parameters.AddWithValue("@Description", (object?)programme.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@EpisodeNum", (object?)programme.EpisodeNum ?? DBNull.Value);
        command.Parameters.AddWithValue("@IconUrl", (object?)programme.IconUrl ?? DBNull.Value);

        var programmeIdObj = await command.ExecuteScalarAsync();
        if (programmeIdObj is long programmeId)
        {
            foreach (var category in programme.Categories)
            {
                using var catCommand = connection.CreateCommand();
                catCommand.Transaction = transaction;
                catCommand.CommandText = @"
                    INSERT INTO ProgrammeCategories (ProgrammeId, Category)
                    VALUES (@ProgrammeId, @Category);
                ";
                catCommand.Parameters.AddWithValue("@ProgrammeId", programmeId);
                catCommand.Parameters.AddWithValue("@Category", category);

                await catCommand.ExecuteNonQueryAsync();
            }
        }
    }
}
