using Viox.Client.Spotify.Models;

namespace Viox.Client.Spotify.Services;

/// <summary>
/// Contract for interacting with the Spotify Web API.
/// </summary>
public interface ISpotifyClient
{
    /// <summary>
    /// Get Spotify catalog information for a single album.
    /// </summary>
    /// <param name="id">The Spotify ID for the album.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing the album details.</returns>
    Task<SpotifyResponse<SpotifyAlbum>> GetAlbumAsync(string id, string? market = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Spotify catalog information for multiple albums identified by their Spotify IDs.
    /// </summary>
    /// <param name="ids">A list of the Spotify IDs for the albums.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a list of albums.</returns>
    [Obsolete("This endpoint is marked as deprecated by Spotify.")]
    Task<SpotifyResponse<SpotifyAlbumList>> GetSeveralAlbumsAsync(IEnumerable<string> ids, string? market = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Spotify catalog information about an album's tracks.
    /// </summary>
    /// <param name="id">The Spotify ID for the album.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged set of tracks.</returns>
    Task<SpotifyResponse<SpotifyPagedResult<SpotifyTrack>>> GetAlbumTracksAsync(string id, string? market = null, int? limit = null, int? offset = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Spotify catalog information for a single artist identified by their unique Spotify ID.
    /// </summary>
    /// <param name="id">The Spotify ID for the artist.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing artist information.</returns>
    Task<SpotifyResponse<SpotifyArtist>> GetArtistAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Spotify catalog information for several artists based on their Spotify IDs.
    /// </summary>
    /// <param name="ids">A collection of Spotify IDs for the artists.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a list of artists.</returns>
    [Obsolete("This endpoint is marked as deprecated by Spotify.")]
    Task<SpotifyResponse<SpotifyArtistList>> GetSeveralArtistsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);

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
    Task<SpotifyResponse<SpotifyPagedResult<SpotifyAlbum>>> GetArtistAlbumsAsync(string id, string? includeGroups = null, string? market = null, int? limit = 5, int? offset = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Spotify catalog information about an artist's top tracks by country.
    /// </summary>
    /// <param name="id">The Spotify ID for the artist.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a list of top tracks.</returns>
    [Obsolete("This endpoint is marked as deprecated by Spotify.")]
    Task<SpotifyResponse<SpotifyTrackList>> GetArtistTopTracksAsync(string id, string? market = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Spotify catalog information about artists similar to a given artist.
    /// </summary>
    /// <param name="id">The Spotify ID for the artist.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a list of related artists.</returns>
    [Obsolete("This endpoint is marked as deprecated by Spotify.")]
    Task<SpotifyResponse<SpotifyArtistList>> GetArtistRelatedArtistsAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Spotify catalog information for a single show identified by its unique Spotify ID.
    /// </summary>
    /// <param name="id">The Spotify ID for the show.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing show information.</returns>
    Task<SpotifyResponse<SpotifyShow>> GetShowAsync(string id, string? market = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Spotify catalog information about a show's episodes.
    /// </summary>
    /// <param name="id">The Spotify ID for the show.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged set of episodes.</returns>
    Task<SpotifyResponse<SpotifyPagedResult<SpotifyEpisode>>> GetShowEpisodesAsync(string id, string? market = null, int? limit = null, int? offset = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Spotify catalog information for a single episode identified by its unique Spotify ID.
    /// </summary>
    /// <param name="id">The Spotify ID for the episode.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing episode information.</returns>
    Task<SpotifyResponse<SpotifyEpisode>> GetEpisodeAsync(string id, string? market = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Spotify catalog information for a single audiobook.
    /// </summary>
    /// <param name="id">The Spotify ID for the audiobook.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing audiobook details.</returns>
    Task<SpotifyResponse<SpotifyAudiobook>> GetAudiobookAsync(string id, string? market = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Spotify catalog information about an audiobook's chapters.
    /// </summary>
    /// <param name="id">The Spotify ID for the audiobook.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged set of audiobook chapters.</returns>
    Task<SpotifyResponse<SpotifyPagedResult<SpotifyChapter>>> GetAudiobookChaptersAsync(string id, string? market = null, int? limit = null, int? offset = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a list of the audiobooks saved in the current Spotify user's library.
    /// </summary>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged set of saved audiobooks.</returns>
    Task<SpotifyResponse<SpotifyPagedResult<SpotifyAudiobook>>> GetUsersSavedAudiobooksAsync(int? limit = null, int? offset = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Spotify catalog information for a single audiobook chapter.
    /// </summary>
    /// <param name="id">The Spotify ID for the chapter.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing chapter details.</returns>
    Task<SpotifyResponse<SpotifyChapter>> GetChapterAsync(string id, string? market = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Spotify catalog information for a single track identified by its unique Spotify ID.
    /// </summary>
    /// <param name="id">The Spotify ID for the track.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing track details.</returns>
    Task<SpotifyResponse<SpotifyTrack>> GetTrackAsync(string id, string? market = null, CancellationToken cancellationToken = default);

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
    Task<SpotifyResponse<SpotifySearchResult>> SearchAsync(string query, IEnumerable<string> types, string? market = null, int? limit = null, int? offset = null, string? includeExternal = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed profile information about the current user.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing current user profile details.</returns>
    Task<SpotifyResponse<SpotifyUserProfile>> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a playlist owned by a Spotify user.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="fields">Filters for the fields to return.</param>
    /// <param name="additionalTypes">A comma-separated list of item types (track or episode).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing the playlist details.</returns>
    Task<SpotifyResponse<SpotifyPlaylist>> GetPlaylistAsync(string playlistId, string? market = null, string? fields = null, string? additionalTypes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Change a playlist's name and public/private state.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="request">The payload containing detail updates such as name, description, or public status.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper indicating the result of the operation.</returns>
    Task<SpotifyResponse<object>> ChangePlaylistDetailsAsync(string playlistId, ChangePlaylistDetailsRequest request, CancellationToken cancellationToken = default);

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
    Task<SpotifyResponse<SpotifyPagedResult<SpotifyPlaylistItem>>> GetPlaylistItemsAsync(string playlistId, string? market = null, string? fields = null, int? limit = null, int? offset = null, string? additionalTypes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add one or more items to a user's playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="request">The payload containing items to add.</param>
    /// <param name="position">The zero-based position to insert the items.</param>
    /// <param name="uris">A list of Spotify URIs to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a snapshot result.</returns>
    Task<SpotifyResponse<SpotifySnapshotResult>> AddItemsToPlaylistAsync(string playlistId, AddItemsToPlaylistRequest request, int? position = null, IEnumerable<string>? uris = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Either reorder or replace items in a playlist depending on parameters.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="request">The payload containing reorder or replacement configurations.</param>
    /// <param name="uris">A list of Spotify URIs to replace items with.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a snapshot result.</returns>
    Task<SpotifyResponse<SpotifySnapshotResult>> ReorderOrReplacePlaylistItemsAsync(string playlistId, ReorderOrReplacePlaylistItemsRequest request, IEnumerable<string>? uris = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove one or more items from a user's playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="request">The payload containing the items to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a snapshot result.</returns>
    Task<SpotifyResponse<SpotifySnapshotResult>> RemoveItemsFromPlaylistAsync(string playlistId, RemovePlaylistItemsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a list of the playlists owned or followed by the current Spotify user.
    /// </summary>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged list of playlists.</returns>
    Task<SpotifyResponse<SpotifyPagedResult<SpotifyPlaylist>>> GetCurrentUserPlaylistsAsync(int? limit = null, int? offset = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a playlist for the current Spotify user.
    /// </summary>
    /// <param name="request">The payload containing playlist creation specifications.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing the newly created playlist.</returns>
    Task<SpotifyResponse<SpotifyPlaylist>> CreatePlaylistAsync(CreatePlaylistRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add one or more items to the current user's library using Spotify URIs.
    /// </summary>
    /// <param name="uris">A list of Spotify URIs to save.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper indicating the result of the action.</returns>
    Task<SpotifyResponse<object>> SaveLibraryItemsAsync(IEnumerable<string> uris, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove one or more items from the current user's library using Spotify URIs.
    /// </summary>
    /// <param name="uris">A list of Spotify URIs to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper indicating the result of the action.</returns>
    Task<SpotifyResponse<object>> RemoveLibraryItemsAsync(IEnumerable<string> uris, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if one or more items are already saved in the current user's library.
    /// </summary>
    /// <param name="uris">A list of Spotify URIs to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing booleans corresponding to each URI in sequence.</returns>
    Task<SpotifyResponse<IReadOnlyList<bool>>> CheckLibraryContainsAsync(IEnumerable<string> uris, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a list of the albums saved in the current Spotify user's library.
    /// </summary>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged list of saved albums.</returns>
    Task<SpotifyResponse<SpotifyPagedResult<SpotifySavedAlbum>>> GetUsersSavedAlbumsAsync(int? limit = null, int? offset = null, string? market = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a list of the songs saved in the current Spotify user's library.
    /// </summary>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged list of saved tracks.</returns>
    Task<SpotifyResponse<SpotifyPagedResult<SpotifySavedTrack>>> GetUsersSavedTracksAsync(string? market = null, int? limit = null, int? offset = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a list of the episodes saved in the current Spotify user's library.
    /// </summary>
    /// <param name="market">An ISO 3166-1 alpha-2 country code or an item like "from_token".</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged list of saved episodes.</returns>
    Task<SpotifyResponse<SpotifyPagedResult<SpotifySavedEpisode>>> GetUsersSavedEpisodesAsync(string? market = null, int? limit = null, int? offset = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a list of shows saved in the current Spotify user's library.
    /// </summary>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="offset">The index of the first item to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a paged list of saved shows.</returns>
    Task<SpotifyResponse<SpotifyPagedResult<SpotifySavedShow>>> GetUsersSavedShowsAsync(int? limit = null, int? offset = null, CancellationToken cancellationToken = default);

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
    Task<SpotifyResponse<SpotifyPagedResult<T>>> GetUsersTopItemsAsync<T>(string type, string? timeRange = null, int? limit = null, int? offset = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Get the current user's followed artists.
    /// </summary>
    /// <param name="type">The ID type to fetch, currently defaults to "artist".</param>
    /// <param name="after">The last item ID fetched from previous queries.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing cursor-paged artist results.</returns>
    Task<SpotifyResponse<SpotifyCursorPagedResult<SpotifyArtist>>> GetFollowedArtistsAsync(string type = "artist", string? after = null, int? limit = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replace the image used to represent a specific playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="base64JpegImage">Base64 encoded JPEG image data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper indicating the status of the image upload.</returns>
    Task<SpotifyResponse<object>> UploadCustomPlaylistCoverAsync(string playlistId, string base64JpegImage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the current image associated with a specific playlist.
    /// </summary>
    /// <param name="playlistId">The Spotify ID of the playlist.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A response wrapper containing a list of Spotify image metadata objects.</returns>
    Task<SpotifyResponse<IReadOnlyList<SpotifyImage>>> GetPlaylistCoverImageAsync(string playlistId, CancellationToken cancellationToken = default);

    Task<SpotifyResponse<IReadOnlyList<SpotifyDevice>>> GetDevices(CancellationToken ct = default);

}
