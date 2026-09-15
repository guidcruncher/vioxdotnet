using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Spotify.Configuration;
using Viox.Client.Spotify.Models;

namespace Viox.Client.Spotify.Services;

/// <summary>
/// Production ready implementation of Spotify Web API client targeting .NET 10.
/// </summary>
public sealed class SpotifyClient : ISpotifyClient
{
    /// <summary>
    /// The HTTP client instance used for executing requests against the Spotify API.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Service used to retrieve and refresh Spotify access tokens.
    /// </summary>
    private readonly ISpotifyAuthService _authService;

    /// <summary>
    /// Logger instance used to capture execution trace data and HTTP errors.
    /// </summary>
    private readonly ILogger<SpotifyClient> _logger;

    /// <summary>
    /// Configuration options specifying API settings such as credentials, base address, and timeouts.
    /// </summary>
    private readonly SpotifyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpotifyClient"/> class using Microsoft Extensions dependency injection components.
    /// </summary>
    /// <param name="httpClient">The HTTP client instance provided by <see cref="IHttpClientFactory"/> or dependency injection.</param>
    /// <param name="authService">The Spotify authentication service responsible for access token lifecycle.</param>
    /// <param name="options">The configured options instance containing connection and authorization metadata.</param>
    /// <param name="logger">The logger for recording runtime diagnostic messages and errors.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is <see langword="null"/>.</exception>
    public SpotifyClient(
        HttpClient httpClient,
        ISpotifyAuthService authService,
        IOptions<SpotifyOptions> options,
        ILogger<SpotifyClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        ConfigureHttpClient();
    }

    /// <summary>
    /// Configures default properties of the underlying <see cref="HttpClient"/> instance, including base address and request timeout.
    /// </summary>
    private void ConfigureHttpClient()
    {
        if (_httpClient.BaseAddress is null)
        {
            _httpClient.BaseAddress = _options.BaseAddress;
        }

        if (_options.Timeout != TimeSpan.Zero)
        {
            _httpClient.Timeout = _options.Timeout;
        }
    }

    /// <summary>
    /// Get Spotify catalog information for a single album.
    /// </summary>
    /// <param name="id">The Spotify ID for the album.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing the album details.</returns>
    public async Task<SpotifyResponse<SpotifyAlbum>> GetAlbumAsync(string id, string? market = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");

        var path = BuildPath($"albums/{Uri.EscapeDataString(id)}", queryParams);
        return await SendAsync<SpotifyAlbum>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information for multiple albums identified by their Spotify IDs.
    /// </summary>
    /// <param name="ids">A list of the Spotify IDs for the albums.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a list of albums.</returns>
    [Obsolete("This endpoint is marked as deprecated by Spotify.")]
    public async Task<SpotifyResponse<SpotifyAlbumList>> GetSeveralAlbumsAsync(IEnumerable<string> ids, string? market = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string> { $"ids={Uri.EscapeDataString(string.Join(',', ids))}" };
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");

        var path = BuildPath("albums", queryParams);
        return await SendAsync<SpotifyAlbumList>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information about an album's tracks.
    /// </summary>
    /// <param name="id">The Spotify ID for the album.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged set of tracks.</returns>
    public async Task<SpotifyResponse<SpotifyPagedResult<SpotifyTrack>>> GetAlbumTracksAsync(string id, string? market = null, int? limit = null, int? offset = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (offset.HasValue) queryParams.Add($"offset={offset.Value}");

        var path = BuildPath($"albums/{Uri.EscapeDataString(id)}/tracks", queryParams);
        return await SendAsync<SpotifyPagedResult<SpotifyTrack>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information for a single artist identified by their unique Spotify ID.
    /// </summary>
    /// <param name="id">The Spotify ID for the artist.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing artist information.</returns>
    public async Task<SpotifyResponse<SpotifyArtist>> GetArtistAsync(string id, CancellationToken cancellationToken = default)
    {
        var path = $"artists/{Uri.EscapeDataString(id)}";
        return await SendAsync<SpotifyArtist>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information for several artists based on their Spotify IDs.
    /// </summary>
    /// <param name="ids">A collection of Spotify IDs for the artists.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a list of artists.</returns>
    [Obsolete("This endpoint is marked as deprecated by Spotify.")]
    public async Task<SpotifyResponse<SpotifyArtistList>> GetSeveralArtistsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        var queryParams = new[] { $"ids={Uri.EscapeDataString(string.Join(',', ids))}" };
        var path = BuildPath("artists", queryParams);
        return await SendAsync<SpotifyArtistList>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information about an artist's albums.
    /// </summary>
    /// <param name="id">The Spotify ID for the artist.</param>
    /// <param name="includeGroups">A comma-separated list of keywords filtering the response (e.g., album, single, appears_on, compilation).</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged set of albums.</returns>
    public async Task<SpotifyResponse<SpotifyPagedResult<SpotifyAlbum>>> GetArtistAlbumsAsync(string id, string? includeGroups = null, string? market = null, int? limit = 5, int? offset = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(includeGroups)) queryParams.Add($"include_groups={Uri.EscapeDataString(includeGroups)}");
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (offset.HasValue) queryParams.Add($"offset={offset.Value}");

        var path = BuildPath($"artists/{Uri.EscapeDataString(id)}/albums", queryParams);
        return await SendAsync<SpotifyPagedResult<SpotifyAlbum>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information about an artist's top tracks by country.
    /// </summary>
    /// <param name="id">The Spotify ID for the artist.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a list of top tracks.</returns>
    [Obsolete("This endpoint is marked as deprecated by Spotify.")]
    public async Task<SpotifyResponse<SpotifyTrackList>> GetArtistTopTracksAsync(string id, string? market = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");

        var path = BuildPath($"artists/{Uri.EscapeDataString(id)}/top-tracks", queryParams);
        return await SendAsync<SpotifyTrackList>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information about artists similar to a given artist.
    /// </summary>
    /// <param name="id">The Spotify ID for the artist.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a list of related artists.</returns>
    [Obsolete("This endpoint is marked as deprecated by Spotify.")]
    public async Task<SpotifyResponse<SpotifyArtistList>> GetArtistRelatedArtistsAsync(string id, CancellationToken cancellationToken = default)
    {
        var path = $"artists/{Uri.EscapeDataString(id)}/related-artists";
        return await SendAsync<SpotifyArtistList>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information for a single show identified by its unique Spotify ID.
    /// </summary>
    /// <param name="id">The Spotify ID for the show.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing show information.</returns>
    public async Task<SpotifyResponse<SpotifyShow>> GetShowAsync(string id, string? market = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");

        var path = BuildPath($"shows/{Uri.EscapeDataString(id)}", queryParams);
        return await SendAsync<SpotifyShow>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information about a show's episodes.
    /// </summary>
    /// <param name="id">The Spotify ID for the show.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged set of episodes.</returns>
    public async Task<SpotifyResponse<SpotifyPagedResult<SpotifyEpisode>>> GetShowEpisodesAsync(string id, string? market = null, int? limit = null, int? offset = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (offset.HasValue) queryParams.Add($"offset={offset.Value}");

        var path = BuildPath($"shows/{Uri.EscapeDataString(id)}/episodes", queryParams);
        return await SendAsync<SpotifyPagedResult<SpotifyEpisode>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information for a single episode identified by its unique Spotify ID.
    /// </summary>
    /// <param name="id">The Spotify ID for the episode.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing episode information.</returns>
    public async Task<SpotifyResponse<SpotifyEpisode>> GetEpisodeAsync(string id, string? market = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");

        var path = BuildPath($"episodes/{Uri.EscapeDataString(id)}", queryParams);
        return await SendAsync<SpotifyEpisode>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information for a single audiobook.
    /// </summary>
    /// <param name="id">The Spotify ID for the audiobook.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing audiobook details.</returns>
    public async Task<SpotifyResponse<SpotifyAudiobook>> GetAudiobookAsync(string id, string? market = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");

        var path = BuildPath($"audiobooks/{Uri.EscapeDataString(id)}", queryParams);
        return await SendAsync<SpotifyAudiobook>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information about an audiobook's chapters.
    /// </summary>
    /// <param name="id">The Spotify ID for the audiobook.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged set of audiobook chapters.</returns>
    public async Task<SpotifyResponse<SpotifyPagedResult<SpotifyChapter>>> GetAudiobookChaptersAsync(string id, string? market = null, int? limit = null, int? offset = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (offset.HasValue) queryParams.Add($"offset={offset.Value}");

        var path = BuildPath($"audiobooks/{Uri.EscapeDataString(id)}/chapters", queryParams);
        return await SendAsync<SpotifyPagedResult<SpotifyChapter>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get a list of the audiobooks saved in the current Spotify user's library.
    /// </summary>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged set of saved audiobooks.</returns>
    public async Task<SpotifyResponse<SpotifyPagedResult<SpotifyAudiobook>>> GetUsersSavedAudiobooksAsync(int? limit = null, int? offset = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (offset.HasValue) queryParams.Add($"offset={offset.Value}");

        var path = BuildPath("me/audiobooks", queryParams);
        return await SendAsync<SpotifyPagedResult<SpotifyAudiobook>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information for a single audiobook chapter.
    /// </summary>
    /// <param name="id">The Spotify ID for the chapter.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing chapter details.</returns>
    public async Task<SpotifyResponse<SpotifyChapter>> GetChapterAsync(string id, string? market = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");

        var path = BuildPath($"chapters/{Uri.EscapeDataString(id)}", queryParams);
        return await SendAsync<SpotifyChapter>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information for a single track identified by its unique Spotify ID.
    /// </summary>
    /// <param name="id">The Spotify ID for the track.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing track details.</returns>
    public async Task<SpotifyResponse<SpotifyTrack>> GetTrackAsync(string id, string? market = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");

        var path = BuildPath($"tracks/{Uri.EscapeDataString(id)}", queryParams);
        return await SendAsync<SpotifyTrack>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get Spotify catalog information about albums, artists, playlists, tracks, shows, episodes or audiobooks matching a query string.
    /// </summary>
    /// <param name="query">Your search query keyword.</param>
    /// <param name="types">A list of item types to search across (e.g., album, artist, playlist, track, show, episode, audiobook).</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="includeExternal">If given, response will include any relevant external content.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing the consolidated search result.</returns>
    public async Task<SpotifyResponse<SpotifySearchResult>> SearchAsync(string query, IEnumerable<string> types, string? market = null, int? limit = null, int? offset = null, string? includeExternal = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>
        {
            $"q={Uri.EscapeDataString(query)}",
            $"type={Uri.EscapeDataString(string.Join(',', types))}"
        };

        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (offset.HasValue) queryParams.Add($"offset={offset.Value}");
        if (!string.IsNullOrWhiteSpace(includeExternal)) queryParams.Add($"include_external={Uri.EscapeDataString(includeExternal)}");

        var path = BuildPath("search", queryParams);
        return await SendAsync<SpotifySearchResult>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get detailed profile information about the current user.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing current user profile details.</returns>
    public async Task<SpotifyResponse<SpotifyUserProfile>> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default)
    {
        return await SendAsync<SpotifyUserProfile>(HttpMethod.Get, "me", null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get a playlist owned by a Spotify user.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="fields">Filters for the fields to return.</param>
    /// <param name="additionalTypes">A comma-separated list of item types (track or episode).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing the playlist details.</returns>
    public async Task<SpotifyResponse<SpotifyPlaylist>> GetPlaylistAsync(string playlistId, string? market = null, string? fields = null, string? additionalTypes = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");
        if (!string.IsNullOrWhiteSpace(fields)) queryParams.Add($"fields={Uri.EscapeDataString(fields)}");
        if (!string.IsNullOrWhiteSpace(additionalTypes)) queryParams.Add($"additional_types={Uri.EscapeDataString(additionalTypes)}");

        var path = BuildPath($"playlists/{Uri.EscapeDataString(playlistId)}", queryParams);
        return await SendAsync<SpotifyPlaylist>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Change a playlist's name and public/private state.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="request">The payload containing detail updates such as name, description, or public status.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper indicating the result of the operation.</returns>
    public async Task<SpotifyResponse<object>> ChangePlaylistDetailsAsync(string playlistId, ChangePlaylistDetailsRequest request, CancellationToken cancellationToken = default)
    {
        var path = $"playlists/{Uri.EscapeDataString(playlistId)}";
        return await SendAsync<object>(HttpMethod.Put, path, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get full details of the items of a playlist owned by a Spotify user.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="fields">Filters for the fields to return.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="additionalTypes">A comma-separated list of item types (track or episode).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged set of playlist items.</returns>
    public async Task<SpotifyResponse<SpotifyPagedResult<SpotifyPlaylistItem>>> GetPlaylistItemsAsync(string playlistId, string? market = null, string? fields = null, int? limit = null, int? offset = null, string? additionalTypes = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");
        if (!string.IsNullOrWhiteSpace(fields)) queryParams.Add($"fields={Uri.EscapeDataString(fields)}");
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (offset.HasValue) queryParams.Add($"offset={offset.Value}");
        if (!string.IsNullOrWhiteSpace(additionalTypes)) queryParams.Add($"additional_types={Uri.EscapeDataString(additionalTypes)}");

        var path = BuildPath($"playlists/{Uri.EscapeDataString(playlistId)}/items", queryParams);
        return await SendAsync<SpotifyPagedResult<SpotifyPlaylistItem>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Add one or more items to a user's playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="request">The payload containing items to add.</param>
    /// <param name="position">The zero-based position to insert the items.</param>
    /// <param name="uris">A list of Spotify URIs to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a snapshot result.</returns>
    public async Task<SpotifyResponse<SpotifySnapshotResult>> AddItemsToPlaylistAsync(string playlistId, AddItemsToPlaylistRequest request, int? position = null, IEnumerable<string>? uris = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (position.HasValue) queryParams.Add($"position={position.Value}");
        if (uris is not null && uris.Any()) queryParams.Add($"uris={Uri.EscapeDataString(string.Join(',', uris))}");

        var path = BuildPath($"playlists/{Uri.EscapeDataString(playlistId)}/items", queryParams);
        return await SendAsync<SpotifySnapshotResult>(HttpMethod.Post, path, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Either reorder or replace items in a playlist depending on parameters.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="request">The payload containing reorder or replacement configurations.</param>
    /// <param name="uris">A list of Spotify URIs to replace items with.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a snapshot result.</returns>
    public async Task<SpotifyResponse<SpotifySnapshotResult>> ReorderOrReplacePlaylistItemsAsync(string playlistId, ReorderOrReplacePlaylistItemsRequest request, IEnumerable<string>? uris = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (uris is not null && uris.Any()) queryParams.Add($"uris={Uri.EscapeDataString(string.Join(',', uris))}");

        var path = BuildPath($"playlists/{Uri.EscapeDataString(playlistId)}/items", queryParams);
        return await SendAsync<SpotifySnapshotResult>(HttpMethod.Put, path, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Remove one or more items from a user's playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="request">The payload containing the items to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a snapshot result.</returns>
    public async Task<SpotifyResponse<SpotifySnapshotResult>> RemoveItemsFromPlaylistAsync(string playlistId, RemovePlaylistItemsRequest request, CancellationToken cancellationToken = default)
    {
        var path = $"playlists/{Uri.EscapeDataString(playlistId)}/items";
        return await SendAsync<SpotifySnapshotResult>(HttpMethod.Delete, path, request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get a list of the playlists owned or followed by the current Spotify user.
    /// </summary>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged list of playlists.</returns>
    public async Task<SpotifyResponse<SpotifyPagedResult<SpotifyPlaylist>>> GetCurrentUserPlaylistsAsync(int? limit = null, int? offset = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (offset.HasValue) queryParams.Add($"offset={offset.Value}");

        var path = BuildPath("me/playlists", queryParams);
        return await SendAsync<SpotifyPagedResult<SpotifyPlaylist>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Create a playlist for the current Spotify user.
    /// </summary>
    /// <param name="request">The payload containing playlist creation specifications.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing the newly created playlist.</returns>
    public async Task<SpotifyResponse<SpotifyPlaylist>> CreatePlaylistAsync(CreatePlaylistRequest request, CancellationToken cancellationToken = default)
    {
        return await SendAsync<SpotifyPlaylist>(HttpMethod.Post, "me/playlists", request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Add one or more items to the current user's library using Spotify URIs.
    /// </summary>
    /// <param name="uris">A list of Spotify URIs to save.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper indicating the result of the action.</returns>
    public async Task<SpotifyResponse<object>> SaveLibraryItemsAsync(IEnumerable<string> uris, CancellationToken cancellationToken = default)
    {
        var queryParams = new[] { $"uris={Uri.EscapeDataString(string.Join(',', uris))}" };
        var path = BuildPath("me/library", queryParams);
        return await SendAsync<object>(HttpMethod.Put, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Remove one or more items from the current user's library using Spotify URIs.
    /// </summary>
    /// <param name="uris">A list of Spotify URIs to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper indicating the result of the action.</returns>
    public async Task<SpotifyResponse<object>> RemoveLibraryItemsAsync(IEnumerable<string> uris, CancellationToken cancellationToken = default)
    {
        var queryParams = new[] { $"uris={Uri.EscapeDataString(string.Join(',', uris))}" };
        var path = BuildPath("me/library", queryParams);
        return await SendAsync<object>(HttpMethod.Delete, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Check if one or more items are already saved in the current user's library.
    /// </summary>
    /// <param name="uris">A list of Spotify URIs to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing booleans corresponding to each URI in sequence.</returns>
    public async Task<SpotifyResponse<IReadOnlyList<bool>>> CheckLibraryContainsAsync(IEnumerable<string> uris, CancellationToken cancellationToken = default)
    {
        var queryParams = new[] { $"uris={Uri.EscapeDataString(string.Join(',', uris))}" };
        var path = BuildPath("me/library/contains", queryParams);
        return await SendAsync<IReadOnlyList<bool>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get a list of the albums saved in the current Spotify user's library.
    /// </summary>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged list of saved albums.</returns>
    public async Task<SpotifyResponse<SpotifyPagedResult<SpotifySavedAlbum>>> GetUsersSavedAlbumsAsync(int? limit = null, int? offset = null, string? market = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (offset.HasValue) queryParams.Add($"offset={offset.Value}");
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");

        var path = BuildPath("me/albums", queryParams);
        return await SendAsync<SpotifyPagedResult<SpotifySavedAlbum>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get a list of the songs saved in the current Spotify user's library.
    /// </summary>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged list of saved tracks.</returns>
    public async Task<SpotifyResponse<SpotifyPagedResult<SpotifySavedTrack>>> GetUsersSavedTracksAsync(string? market = null, int? limit = null, int? offset = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (offset.HasValue) queryParams.Add($"offset={offset.Value}");

        var path = BuildPath("me/tracks", queryParams);
        return await SendAsync<SpotifyPagedResult<SpotifySavedTrack>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get a list of the episodes saved in the current Spotify user's library.
    /// </summary>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged list of saved episodes.</returns>
    public async Task<SpotifyResponse<SpotifyPagedResult<SpotifySavedEpisode>>> GetUsersSavedEpisodesAsync(string? market = null, int? limit = null, int? offset = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(market)) queryParams.Add($"market={Uri.EscapeDataString(market)}");
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (offset.HasValue) queryParams.Add($"offset={offset.Value}");

        var path = BuildPath("me/episodes", queryParams);
        return await SendAsync<SpotifyPagedResult<SpotifySavedEpisode>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get a list of shows saved in the current Spotify user's library.
    /// </summary>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged list of saved shows.</returns>
    public async Task<SpotifyResponse<SpotifyPagedResult<SpotifySavedShow>>> GetUsersSavedShowsAsync(int? limit = null, int? offset = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (offset.HasValue) queryParams.Add($"offset={offset.Value}");

        var path = BuildPath("me/shows", queryParams);
        return await SendAsync<SpotifyPagedResult<SpotifySavedShow>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get the current user's top artists or tracks based on calculated affinity.
    /// </summary>
    /// <typeparam name="T">The targeted return item model type (e.g. SpotifyArtist or SpotifyTrack).</typeparam>
    /// <param name="type">The target entity type (e.g. "artists" or "tracks").</param>
    /// <param name="timeRange">Over what time frame the affinity data is computed (e.g., long_term, medium_term, short_term).</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged list of top items of type <typeparamref name="T"/>.</returns>
    public async Task<SpotifyResponse<SpotifyPagedResult<T>>> GetUsersTopItemsAsync<T>(string type, string? timeRange = null, int? limit = null, int? offset = null, CancellationToken cancellationToken = default) where T : class
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(timeRange)) queryParams.Add($"time_range={Uri.EscapeDataString(timeRange)}");
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (offset.HasValue) queryParams.Add($"offset={offset.Value}");

        var path = BuildPath($"me/top/{Uri.EscapeDataString(type)}", queryParams);
        return await SendAsync<SpotifyPagedResult<T>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get the current user's followed artists.
    /// </summary>
    /// <param name="type">The ID type to fetch, currently defaults to "artist".</param>
    /// <param name="after">The last item ID fetched from previous queries.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing cursor-paged artist results.</returns>
    public async Task<SpotifyResponse<SpotifyCursorPagedResult<SpotifyArtist>>> GetFollowedArtistsAsync(string type = "artist", string? after = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string> { $"type={Uri.EscapeDataString(type)}" };
        if (!string.IsNullOrWhiteSpace(after)) queryParams.Add($"after={Uri.EscapeDataString(after)}");
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");

        var path = BuildPath("me/following", queryParams);
        return await SendAsync<SpotifyCursorPagedResult<SpotifyArtist>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Replace the image used to represent a specific playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="base64JpegImage">Base64 encoded JPEG image data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper indicating the status of the image upload.</returns>
    public async Task<SpotifyResponse<object>> UploadCustomPlaylistCoverAsync(string playlistId, string base64JpegImage, CancellationToken cancellationToken = default)
    {
        var path = $"playlists/{Uri.EscapeDataString(playlistId)}/images";
        return await SendRawAsync<object>(HttpMethod.Put, path, new StringContent(base64JpegImage, Encoding.UTF8, "image/jpeg"), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Get the current image associated with a specific playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a list of Spotify image metadata objects.</returns>
    public async Task<SpotifyResponse<IReadOnlyList<SpotifyImage>>> GetPlaylistCoverImageAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        var path = $"playlists/{Uri.EscapeDataString(playlistId)}/images";
        return await SendAsync<IReadOnlyList<SpotifyImage>>(HttpMethod.Get, path, null, cancellationToken).ConfigureAwait(false);
    }

    public async Task<SpotifyResponse<IReadOnlyList<SpotifyDevice>>> GetDevices(CancellationToken ct = default)
    {
        var path = $"me/player/devices";
        return await SendAsync<IReadOnlyList<SpotifyDevice>>(HttpMethod.Get, path, null, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Prepares and dispatches an HTTP request with an optional JSON body payload.
    /// </summary>
    /// <typeparam name="T">The expected response payload type.</typeparam>
    /// <param name="method">The HTTP method to use for the request.</param>
    /// <param name="requestUri">The request URI or endpoint path.</param>
    /// <param name="body">An optional payload object to serialize as JSON content in the HTTP request body.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing the deserialized result or error state.</returns>
    private async Task<SpotifyResponse<T>> SendAsync<T>(HttpMethod method, string requestUri, object? body = null, CancellationToken cancellationToken = default)
    {
        HttpContent? content = body is not null ? JsonContent.Create(body) : null;
        return await SendRawAsync<T>(method, requestUri, content, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the underlying HTTP request, attached Bearer authorization headers dynamically, and refreshes the token using <see cref="ISpotifyAuthService.RefreshTokenAsync"/> on 401 response status codes.
    /// </summary>
    /// <typeparam name="T">The expected response payload type.</typeparam>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="requestUri">The relative request URI.</param>
    /// <param name="content">The request content.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A structured response object wrapping the status code, content, or deserialized error object.</returns>
    private async Task<SpotifyResponse<T>> SendRawAsync<T>(HttpMethod method, string requestUri, HttpContent? content, CancellationToken cancellationToken)
    {
        var accessToken = await _authService.GetAccessToken(cancellationToken).ConfigureAwait(false);
        var response = await ExecuteHttpRequestAsync(method, requestUri, content, accessToken, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogInformation("Request returned 401 Unauthorized. Refreshing token and retrying endpoint: {Uri}", requestUri);

            response.Dispose();
            var refreshToken = await _authService.GetRefreshToken(cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var refreshResult = await _authService.RefreshTokenAsync(refreshToken, cancellationToken).ConfigureAwait(false);
                if (refreshResult is not null)
                {
                    accessToken = refreshResult.AccessToken;
                }
            }

            response = await ExecuteHttpRequestAsync(method, requestUri, content, accessToken, cancellationToken).ConfigureAwait(false);
        }

        try
        {
            if (response.IsSuccessStatusCode)
            {
                if (typeof(T) == typeof(object))
                {
                    return new SpotifyResponse<T>(true, (int)response.StatusCode, default, null);
                }

                var data = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken).ConfigureAwait(false);
                return new SpotifyResponse<T>(true, (int)response.StatusCode, data, null);
            }

            SpotifyErrorObject? errorObject = null;
            try
            {
                var errorWrapper = await response.Content.ReadFromJsonAsync<SpotifyErrorWrapper>(cancellationToken: cancellationToken).ConfigureAwait(false);
                errorObject = errorWrapper?.Error;
            }
            catch
            {
                _logger.LogWarning("Failed to deserialize Spotify API error response for URL: {Url}", requestUri);
            }

            return new SpotifyResponse<T>(false, (int)response.StatusCode, default, errorObject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP request failed for Spotify Client executing path: {Url}", requestUri);
            throw;
        }
        finally
        {
            response.Dispose();
        }
    }

    /// <summary>
    /// Dispatches a single HTTP request with the provided access token header.
    /// </summary>
    private async Task<HttpResponseMessage> ExecuteHttpRequestAsync(HttpMethod method, string requestUri, HttpContent? content, string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, requestUri);

        if (content is not null)
        {
            request.Content = content;
        }

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Concatenates an endpoint path with formatted query parameters.
    /// </summary>
    /// <param name="path">Base relative route path.</param>
    /// <param name="queryParams">A collection of formatted key-value parameter strings.</param>
    /// <returns>The combined path string formatted with a query string prefix if parameters are present.</returns>
    private static string BuildPath(string path, IEnumerable<string> queryParams)
    {
        var list = queryParams.ToList();
        if (list.Count == 0) return path;

        return $"{path}?{string.Join('&', list)}";
    }
}
