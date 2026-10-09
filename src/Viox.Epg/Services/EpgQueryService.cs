namespace Viox.Epg.Services;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Epg.Configuration;
using Viox.Epg.Models;

public class EpgQueryService
{
    private readonly EpgOptions _options;
    private readonly ILogger<EpgQueryService> _logger;

    public EpgQueryService(IOptions<EpgOptions> options, ILogger<EpgQueryService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<CurrentProgrammeRecord>> GetNowPlayingAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<CurrentProgrammeRecord>();
        var currentEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        using var connection = new SqliteConnection(_options.SqliteConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                c.Id AS ChannelId,
                c.DisplayName AS ChannelName,
                c.IconUrl AS ChannelIcon,
                p.Title,
                p.SubTitle,
                p.Description,
                p.Start,
                p.Stop,
                p.EpisodeNum,
                p.IconUrl AS ProgrammeIcon
            FROM Programmes p
            JOIN Channels c ON p.ChannelId = c.Id
            WHERE @CurrentEpoch >= p.Start
              AND @CurrentEpoch < p.Stop
            ORDER BY c.DisplayName;
        ";
        command.Parameters.AddWithValue("@CurrentEpoch", currentEpoch);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(MapRecord(reader));
        }

        return results;
    }

    public async Task<CurrentProgrammeRecord?> GetNowPlayingByChannelAsync(string channelId, CancellationToken cancellationToken = default)
    {
        var currentEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        using var connection = new SqliteConnection(_options.SqliteConnectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT 
                c.Id AS ChannelId,
                c.DisplayName AS ChannelName,
                c.IconUrl AS ChannelIcon,
                p.Title,
                p.SubTitle,
                p.Description,
                p.Start,
                p.Stop,
                p.EpisodeNum,
                p.IconUrl AS ProgrammeIcon
            FROM Programmes p
            JOIN Channels c ON p.ChannelId = c.Id
            WHERE c.Id = @ChannelId
              AND @CurrentEpoch >= p.Start
              AND @CurrentEpoch < p.Stop
            LIMIT 1;
        ";
        command.Parameters.AddWithValue("@ChannelId", channelId);
        command.Parameters.AddWithValue("@CurrentEpoch", currentEpoch);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return MapRecord(reader);
        }

        return null;
    }

    private static CurrentProgrammeRecord MapRecord(SqliteDataReader reader)
    {
        return new CurrentProgrammeRecord
        {
            ChannelId = reader.GetString(0),
            ChannelName = reader.GetString(1),
            ChannelIcon = reader.IsDBNull(2) ? null : reader.GetString(2),
            Title = reader.IsDBNull(3) ? null : reader.GetString(3),
            SubTitle = reader.IsDBNull(4) ? null : reader.GetString(4),
            Description = reader.IsDBNull(5) ? null : reader.GetString(5),
            Start = reader.GetInt64(6),
            Stop = reader.GetInt64(7),
            EpisodeNum = reader.IsDBNull(8) ? null : reader.GetString(8),
            ProgrammeIcon = reader.IsDBNull(9) ? null : reader.GetString(9)
        };
    }
}
