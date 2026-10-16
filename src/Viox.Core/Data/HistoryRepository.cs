namespace Viox.Core.Data;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;
using Viox.Core.Entities;
using Viox.Core.Models;

public class HistoryRepository
{
    private readonly DataOptions _options;
    private readonly ILogger<HistoryRepository> _logger;

    public HistoryRepository(IOptions<DataOptions> options, ILogger<HistoryRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<long> Add(MediaMetaData item, long durationSeconds, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Inserting history record with Uri {uri}.", item.RawUri);
        using var connection = new SqliteConnection(_options.StatsConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO History (Uri, Metadata, DurationSeconds, CreatedTimestamp)
            VALUES (@Uri, @Metadata, @DurationSeconds, @CreatedTimestamp);
            SELECT last_insert_rowid();
        ";

        HistoryItem value = HistoryItem.Create(item);
        value.DurationSeconds = durationSeconds;

        command.Parameters.AddWithValue("@Uri", value.Uri);
        command.Parameters.AddWithValue("@Ketadata", value.Metadata);
        command.Parameters.AddWithValue("@CreatedTimestamp", value.CreatedTimestamp);
        command.Parameters.AddWithValue("@DurationSeconds", value.DurationSeconds);

        var result = command.ExecuteScalar();

        long insertedId = result is not null ? Convert.ToInt64(result) : 0;

        _logger.LogInformation("Successfully inserted history record with ID {InsertedId}.", insertedId);

        return insertedId;
    }

    public async Task<long> UpdateDuration(long id, long durationSeconds, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating duration for ID {id}", id);
        using var connection = new SqliteConnection(_options.StatsConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE History
               SET DurationSeconds = @DurationSeconds
            WHERE Id = @Id
        ";

        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@DurationSeconds", durationSeconds);

        await command.ExecuteNonQueryAsync();

        _logger.LogInformation("Successfully updated duration for ID {id}", id);
        return id;
    }

}
