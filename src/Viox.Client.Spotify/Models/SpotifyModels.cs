using System.Text.Json.Serialization;

namespace Viox.Client.Spotify.Models;

/// <summary>
/// Represents a standardized API response wrapper containing status information and optional data or error details.
/// </summary>
/// <typeparam name="T">The type of payload returned by the API operation.</typeparam>
/// <param name="IsSuccess">Indicates whether the API request completed with a success status code.</param>
/// <param name="StatusCode">The HTTP status code returned by the server.</param>
/// <param name="Data">The deserialized payload object when the request succeeds; otherwise, <see langword="null"/>.</param>
/// <param name="Error">The structured Spotify error response object when the request fails; otherwise, <see langword="null"/>.</param>
public sealed record SpotifyResponse<T>(
    bool IsSuccess,
    int StatusCode,
    T? Data,
    SpotifyErrorObject? Error
);

/// <summary>
/// Represents error payload details returned by the Spotify API on failure.
/// </summary>
public sealed class SpotifyErrorObject
{
    /// <summary>
    /// Gets or sets the HTTP status code embedded within the error body.
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    /// Gets or sets the descriptive error message detailing the reason for failure.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Container wrapping the top-level error object structure returned by Spotify.
/// </summary>
public sealed class SpotifyErrorWrapper
{
    /// <summary>
    /// Gets or sets the inner error details object.
    /// </summary>
    [JsonPropertyName("error")]
    public SpotifyErrorObject? Error { get; set; }
}

/// <summary>
/// Represents a offset-based paginated result set of items returned by Spotify APIs.
/// </summary>
/// <typeparam name="T">The type of elements contained in the paginated item list.</typeparam>
public class SpotifyPagedResult<T>
{
    /// <summary>
    /// Gets or sets a link to the Web API endpoint returning the full details of the result.
    /// </summary>
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum number of items requested or returned.
    /// </summary>
    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    /// <summary>
    /// Gets or sets the URL to the next page of items, or <see langword="null"/> if none available.
    /// </summary>
    [JsonPropertyName("next")]
    public string? Next { get; set; }

    /// <summary>
    /// Gets or sets the offset of the items returned.
    /// </summary>
    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    /// <summary>
    /// Gets or sets the URL to the previous page of items, or <see langword="null"/> if on the first page.
    /// </summary>
    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    /// <summary>
    /// Gets or sets the total number of items available to return.
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>
    /// Gets or sets the requested content items.
    /// </summary>
    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = [];
}

/// <summary>
/// Represents a cursor-based paginated result set of items returned by Spotify APIs.
/// </summary>
/// <typeparam name="T">The type of elements contained in the cursor-paginated item list.</typeparam>
public sealed class SpotifyCursorPagedResult<T>
{
    /// <summary>
    /// Gets or sets a link to the Web API endpoint returning the full details of the result.
    /// </summary>
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum number of items requested or returned.
    /// </summary>
    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    /// <summary>
    /// Gets or sets the URL to the next page of items.
    /// </summary>
    [JsonPropertyName("next")]
    public string? Next { get; set; }

    /// <summary>
    /// Gets or sets key cursors used to calculate offsets for pagination.
    /// </summary>
    [JsonPropertyName("cursors")]
    public SpotifyCursor? Cursors { get; set; }

    /// <summary>
    /// Gets or sets the total number of items available, if provided by the endpoint.
    /// </summary>
    [JsonPropertyName("total")]
    public int? Total { get; set; }

    /// <summary>
    /// Gets or sets the requested content items.
    /// </summary>
    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = [];
}

/// <summary>
/// Holds cursor keys used for moving forward or backward through cursor-paginated responses.
/// </summary>
public sealed class SpotifyCursor
{
    /// <summary>
    /// Gets or sets the cursor to use as key to fetch the next page of items.
    /// </summary>
    [JsonPropertyName("after")]
    public string? After { get; set; }

    /// <summary>
    /// Gets or sets the cursor to use as key to fetch the previous page of items.
    /// </summary>
    [JsonPropertyName("before")]
    public string? Before { get; set; }
}

/// <summary>
/// Represents metadata for an image asset associated with a Spotify resource.
/// </summary>
public sealed class SpotifyImage
{
    /// <summary>
    /// Gets or sets the source URL of the image.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the image height in pixels, if known.
    /// </summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the image width in pixels, if known.
    /// </summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }
}

/// <summary>
/// Contains external public URLs associated with Spotify entities.
/// </summary>
public sealed class SpotifyExternalUrls
{
    /// <summary>
    /// Gets or sets the public Spotify Web URL for the entity.
    /// </summary>
    [JsonPropertyName("spotify")]
    public string Spotify { get; set; } = string.Empty;
}

/// <summary>
/// Holds follower information for an artist or user profile.
/// </summary>
public sealed class SpotifyFollowers
{
    /// <summary>
    /// Gets or sets a link to the Web API endpoint providing follower details, currently unsupported by Spotify.
    /// </summary>
    [JsonPropertyName("href")]
    public string? Href { get; set; }

    /// <summary>
    /// Gets or sets the total number of followers.
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

/// <summary>
/// Represents a Spotify album entity.
/// </summary>
public sealed class SpotifyAlbum
{
    /// <summary>
    /// Gets or sets the unique Spotify ID for the album.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the album.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the album (e.g. "album", "single", "compilation").
    /// </summary>
    [JsonPropertyName("album_type")]
    public string AlbumType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total count of tracks in the album.
    /// </summary>
    [JsonPropertyName("total_tracks")]
    public int TotalTracks { get; set; }

    /// <summary>
    /// Gets or sets the list of markets in which the album is available.
    /// </summary>
    [JsonPropertyName("available_markets")]
    public List<string> AvailableMarkets { get; set; } = [];

    /// <summary>
    /// Gets or sets external URLs associated with this album.
    /// </summary>
    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrls? ExternalUrls { get; set; }

    /// <summary>
    /// Gets or sets a link to the Web API endpoint providing full details of the album.
    /// </summary>
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cover art images for the album in various sizes.
    /// </summary>
    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; } = [];

    /// <summary>
    /// Gets or sets the date the album was released.
    /// </summary>
    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the precision with which release_date value is known (e.g. "year", "month", "day").
    /// </summary>
    [JsonPropertyName("release_date_precision")]
    public string ReleaseDatePrecision { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the object type, which will be "album".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Spotify URI for the album.
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary artists responsible for creating the album.
    /// </summary>
    [JsonPropertyName("artists")]
    public List<SpotifyArtist> Artists { get; set; } = [];

    /// <summary>
    /// Gets or sets the paged track listing for the album, if provided in full endpoint responses.
    /// </summary>
    [JsonPropertyName("tracks")]
    public SpotifyPagedResult<SpotifyTrack>? Tracks { get; set; }
}

/// <summary>
/// Container holding a collection of Spotify albums.
/// </summary>
public sealed class SpotifyAlbumList
{
    /// <summary>
    /// Gets or sets the list of albums.
    /// </summary>
    [JsonPropertyName("albums")]
    public List<SpotifyAlbum> Albums { get; set; } = [];
}

/// <summary>
/// Represents a Spotify artist entity.
/// </summary>
public sealed class SpotifyArtist
{
    /// <summary>
    /// Gets or sets the unique Spotify ID for the artist.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the artist.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets external URLs associated with this artist.
    /// </summary>
    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrls? ExternalUrls { get; set; }

    /// <summary>
    /// Gets or sets information about the followers of the artist.
    /// </summary>
    [JsonPropertyName("followers")]
    public SpotifyFollowers? Followers { get; set; }

    /// <summary>
    /// Gets or sets a list of genres associated with the artist.
    /// </summary>
    [JsonPropertyName("genres")]
    public List<string> Genres { get; set; } = [];

    /// <summary>
    /// Gets or sets a link to the Web API endpoint providing full details of the artist.
    /// </summary>
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets images of the artist in various resolutions.
    /// </summary>
    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; } = [];

    /// <summary>
    /// Gets or sets the popularity score of the artist between 0 and 100.
    /// </summary>
    [JsonPropertyName("popularity")]
    public int Popularity { get; set; }

    /// <summary>
    /// Gets or sets the object type, which will be "artist".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Spotify URI for the artist.
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}

/// <summary>
/// Container holding a collection of Spotify artists.
/// </summary>
public sealed class SpotifyArtistList
{
    /// <summary>
    /// Gets or sets the list of artists.
    /// </summary>
    [JsonPropertyName("artists")]
    public List<SpotifyArtist> Artists { get; set; } = [];
}

/// <summary>
/// Represents a Spotify track entity.
/// </summary>
public sealed class SpotifyTrack
{
    /// <summary>
    /// Gets or sets the unique Spotify ID for the track.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the track.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the artists who performed the track.
    /// </summary>
    [JsonPropertyName("artists")]
    public List<SpotifyArtist> Artists { get; set; } = [];

    /// <summary>
    /// Gets or sets the album on which the track appears.
    /// </summary>
    [JsonPropertyName("album")]
    public SpotifyAlbum? Album { get; set; }

    /// <summary>
    /// Gets or sets the disc number on which the track appears.
    /// </summary>
    [JsonPropertyName("disc_number")]
    public int DiscNumber { get; set; }

    /// <summary>
    /// Gets or sets the track length in milliseconds.
    /// </summary>
    [JsonPropertyName("duration_ms")]
    public int DurationMs { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the track contains explicit lyrics.
    /// </summary>
    [JsonPropertyName("explicit")]
    public bool Explicit { get; set; }

    /// <summary>
    /// Gets or sets external URLs associated with this track.
    /// </summary>
    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrls? ExternalUrls { get; set; }

    /// <summary>
    /// Gets or sets a link to the Web API endpoint providing full details of the track.
    /// </summary>
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the track is playable in the given market.
    /// </summary>
    [JsonPropertyName("is_playable")]
    public bool IsPlayable { get; set; }

    /// <summary>
    /// Gets or sets a link to a 30-second preview (MP3 format) of the track, if available.
    /// </summary>
    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; set; }

    /// <summary>
    /// Gets or sets the track number within its disc/album.
    /// </summary>
    [JsonPropertyName("track_number")]
    public int TrackNumber { get; set; }

    /// <summary>
    /// Gets or sets the object type, which will be "track".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Spotify URI for the track.
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}

/// <summary>
/// Container holding a collection of Spotify tracks.
/// </summary>
public sealed class SpotifyTrackList
{
    /// <summary>
    /// Gets or sets the list of tracks.
    /// </summary>
    [JsonPropertyName("tracks")]
    public List<SpotifyTrack> Tracks { get; set; } = [];
}

/// <summary>
/// Represents a Spotify show (podcast) entity.
/// </summary>
public sealed class SpotifyShow
{
    /// <summary>
    /// Gets or sets the unique Spotify ID for the show.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the show.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description of the show.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the show contains explicit content.
    /// </summary>
    [JsonPropertyName("explicit")]
    public bool Explicit { get; set; }

    /// <summary>
    /// Gets or sets external URLs associated with this show.
    /// </summary>
    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrls? ExternalUrls { get; set; }

    /// <summary>
    /// Gets or sets a link to the Web API endpoint providing full details of the show.
    /// </summary>
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets cover images for the show in various resolutions.
    /// </summary>
    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the show is hosted outside of Spotify.
    /// </summary>
    [JsonPropertyName("is_externally_hosted")]
    public bool IsExternallyHosted { get; set; }

    /// <summary>
    /// Gets or sets a list of the languages used in the show.
    /// </summary>
    [JsonPropertyName("languages")]
    public List<string> Languages { get; set; } = [];

    /// <summary>
    /// Gets or sets the media type of the show (e.g. "audio").
    /// </summary>
    [JsonPropertyName("media_type")]
    public string MediaType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the publisher of the show.
    /// </summary>
    [JsonPropertyName("publisher")]
    public string Publisher { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the object type, which will be "show".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Spotify URI for the show.
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a paged set of episodes belonging to the show.
    /// </summary>
    [JsonPropertyName("episodes")]
    public SpotifyPagedResult<SpotifyEpisode>? Episodes { get; set; }
}

/// <summary>
/// Represents a Spotify podcast episode entity.
/// </summary>
public sealed class SpotifyEpisode
{
    /// <summary>
    /// Gets or sets the unique Spotify ID for the episode.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the episode.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description of the episode.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the duration of the episode in milliseconds.
    /// </summary>
    [JsonPropertyName("duration_ms")]
    public int DurationMs { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the episode contains explicit content.
    /// </summary>
    [JsonPropertyName("explicit")]
    public bool Explicit { get; set; }

    /// <summary>
    /// Gets or sets external URLs associated with this episode.
    /// </summary>
    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrls? ExternalUrls { get; set; }

    /// <summary>
    /// Gets or sets a link to the Web API endpoint providing full details of the episode.
    /// </summary>
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets cover images for the episode in various resolutions.
    /// </summary>
    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the episode is playable in the requested market.
    /// </summary>
    [JsonPropertyName("is_playable")]
    public bool IsPlayable { get; set; }

    /// <summary>
    /// Gets or sets the date the episode was released.
    /// </summary>
    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the object type, which will be "episode".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Spotify URI for the episode.
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}

/// <summary>
/// Represents a Spotify audiobook entity.
/// </summary>
public sealed class SpotifyAudiobook
{
    /// <summary>
    /// Gets or sets the unique Spotify ID for the audiobook.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the audiobook.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of authors of the audiobook.
    /// </summary>
    [JsonPropertyName("authors")]
    public List<SpotifyAuthor> Authors { get; set; } = [];

    /// <summary>
    /// Gets or sets a description of the audiobook.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the audiobook contains explicit content.
    /// </summary>
    [JsonPropertyName("explicit")]
    public bool Explicit { get; set; }

    /// <summary>
    /// Gets or sets external URLs associated with this audiobook.
    /// </summary>
    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrls? ExternalUrls { get; set; }

    /// <summary>
    /// Gets or sets a link to the Web API endpoint providing full details of the audiobook.
    /// </summary>
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets cover images for the audiobook in various resolutions.
    /// </summary>
    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; } = [];

    /// <summary>
    /// Gets or sets a list of languages used in the audiobook.
    /// </summary>
    [JsonPropertyName("languages")]
    public List<string> Languages { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of narrators who voice the audiobook.
    /// </summary>
    [JsonPropertyName("narrators")]
    public List<SpotifyNarrator> Narrators { get; set; } = [];

    /// <summary>
    /// Gets or sets the publisher of the audiobook.
    /// </summary>
    [JsonPropertyName("publisher")]
    public string Publisher { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the object type, which will be "audiobook".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Spotify URI for the audiobook.
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a paged set of chapters belonging to the audiobook.
    /// </summary>
    [JsonPropertyName("chapters")]
    public SpotifyPagedResult<SpotifyChapter>? Chapters { get; set; }
}

/// <summary>
/// Represents author metadata for an audiobook.
/// </summary>
public sealed class SpotifyAuthor
{
    /// <summary>
    /// Gets or sets the author's name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents narrator metadata for an audiobook.
/// </summary>
public sealed class SpotifyNarrator
{
    /// <summary>
    /// Gets or sets the narrator's name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents an individual audiobook chapter entity.
/// </summary>
public sealed class SpotifyChapter
{
    /// <summary>
    /// Gets or sets the unique Spotify ID for the chapter.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the chapter.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description of the chapter.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the duration of the chapter in milliseconds.
    /// </summary>
    [JsonPropertyName("duration_ms")]
    public int DurationMs { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the chapter contains explicit content.
    /// </summary>
    [JsonPropertyName("explicit")]
    public bool Explicit { get; set; }

    /// <summary>
    /// Gets or sets external URLs associated with this chapter.
    /// </summary>
    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrls? ExternalUrls { get; set; }

    /// <summary>
    /// Gets or sets a link to the Web API endpoint providing full details of the chapter.
    /// </summary>
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets cover images for the chapter in various resolutions.
    /// </summary>
    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the chapter is playable in the given market.
    /// </summary>
    [JsonPropertyName("is_playable")]
    public bool IsPlayable { get; set; }

    /// <summary>
    /// Gets or sets the date the chapter was released.
    /// </summary>
    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the object type, which will be "episode".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Spotify URI for the chapter.
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}

/// <summary>
/// Represents consolidated catalog search results categorised by entity type.
/// </summary>
public sealed class SpotifySearchResult
{
    /// <summary>
    /// Gets or sets the matching album search results, if requested.
    /// </summary>
    [JsonPropertyName("albums")]
    public SpotifyPagedResult<SpotifyAlbum>? Albums { get; set; }

    /// <summary>
    /// Gets or sets the matching artist search results, if requested.
    /// </summary>
    [JsonPropertyName("artists")]
    public SpotifyPagedResult<SpotifyArtist>? Artists { get; set; }

    /// <summary>
    /// Gets or sets the matching playlist search results, if requested.
    /// </summary>
    [JsonPropertyName("playlists")]
    public SpotifyPagedResult<SpotifyPlaylist>? Playlists { get; set; }

    /// <summary>
    /// Gets or sets the matching track search results, if requested.
    /// </summary>
    [JsonPropertyName("tracks")]
    public SpotifyPagedResult<SpotifyTrack>? Tracks { get; set; }

    /// <summary>
    /// Gets or sets the matching show search results, if requested.
    /// </summary>
    [JsonPropertyName("shows")]
    public SpotifyPagedResult<SpotifyShow>? Shows { get; set; }

    /// <summary>
    /// Gets or sets the matching episode search results, if requested.
    /// </summary>
    [JsonPropertyName("episodes")]
    public SpotifyPagedResult<SpotifyEpisode>? Episodes { get; set; }

    /// <summary>
    /// Gets or sets the matching audiobook search results, if requested.
    /// </summary>
    [JsonPropertyName("audiobooks")]
    public SpotifyPagedResult<SpotifyAudiobook>? Audiobooks { get; set; }
}

/// <summary>
/// Represents user profile details for a Spotify user.
/// </summary>
public sealed class SpotifyUserProfile
{
    /// <summary>
    /// Gets or sets the unique Spotify user ID for the user.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("account_id")]
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name displayed on the user profile.
    /// </summary>
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the user's email address, requiring appropriate OAuth scopes.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets external URLs associated with this user.
    /// </summary>
    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrls? ExternalUrls { get; set; }

    /// <summary>
    /// Gets or sets information about the user's followers.
    /// </summary>
    [JsonPropertyName("followers")]
    public SpotifyFollowers? Followers { get; set; }

    /// <summary>
    /// Gets or sets a link to the Web API endpoint providing full details of the user profile.
    /// </summary>
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's profile image assets.
    /// </summary>
    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; } = [];

    /// <summary>
    /// Gets or sets the user's Spotify subscription plan type (e.g. "premium", "free").
    /// </summary>
    [JsonPropertyName("product")]
    public string? Product { get; set; }

    /// <summary>
    /// Gets or sets the object type, which will be "user".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Spotify URI for the user.
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}

/// <summary>
/// Represents a Spotify playlist entity.
/// </summary>
public sealed class SpotifyPlaylist
{
    /// <summary>
    /// Gets or sets the unique Spotify ID for the playlist.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the playlist.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the owner allows other users to modify the playlist.
    /// </summary>
    [JsonPropertyName("collaborative")]
    public bool Collaborative { get; set; }

    /// <summary>
    /// Gets or sets the description of the playlist.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets external URLs associated with this playlist.
    /// </summary>
    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrls? ExternalUrls { get; set; }

    /// <summary>
    /// Gets or sets information about the followers of the playlist.
    /// </summary>
    [JsonPropertyName("followers")]
    public SpotifyFollowers? Followers { get; set; }

    /// <summary>
    /// Gets or sets a link to the Web API endpoint providing full details of the playlist.
    /// </summary>
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets custom cover images for the playlist.
    /// </summary>
    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; } = [];

    /// <summary>
    /// Gets or sets the user who owns the playlist.
    /// </summary>
    [JsonPropertyName("owner")]
    public SpotifyUserProfile? Owner { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the playlist is public or private.
    /// </summary>
    [JsonPropertyName("public")]
    public bool? Public { get; set; }

    /// <summary>
    /// Gets or sets the version identifier for the current playlist snapshot.
    /// </summary>
    [JsonPropertyName("snapshot_id")]
    public string SnapshotId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of tracks and items included in the playlist.
    /// </summary>
    [JsonPropertyName("tracks")]
    public SpotifyPagedResult<SpotifyPlaylistItem>? Tracks { get; set; }

    /// <summary>
    /// Gets or sets the object type, which will be "playlist".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Spotify URI for the playlist.
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}

/// <summary>
/// Represents an entry within a playlist, wrapping track/episode details and addition metadata.
/// </summary>
public sealed class SpotifyPlaylistItem
{
    /// <summary>
    /// Gets or sets the date and time when the item was added to the playlist.
    /// </summary>
    [JsonPropertyName("added_at")]
    public DateTime? AddedAt { get; set; }

    /// <summary>
    /// Gets or sets the user profile of the account that added the item to the playlist.
    /// </summary>
    [JsonPropertyName("added_by")]
    public SpotifyUserProfile? AddedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item is a local file rather than a catalog track.
    /// </summary>
    [JsonPropertyName("is_local")]
    public bool IsLocal { get; set; }

    /// <summary>
    /// Gets or sets the track or episode details associated with this playlist item.
    /// </summary>
    [JsonPropertyName("track")]
    public SpotifyTrack? Track { get; set; }
}

/// <summary>
/// Represents a snapshot response returned after modifying a playlist.
/// </summary>
public sealed class SpotifySnapshotResult
{
    /// <summary>
    /// Gets or sets the snapshot ID representing the newly updated state of the playlist.
    /// </summary>
    [JsonPropertyName("snapshot_id")]
    public string SnapshotId { get; set; } = string.Empty;
}

/// <summary>
/// Represents the request payload used to update playlist details such as name or visibility.
/// </summary>
public sealed class ChangePlaylistDetailsRequest
{
    /// <summary>
    /// Gets or sets the new name for the playlist.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the playlist will be public.
    /// </summary>
    [JsonPropertyName("public")]
    public bool? Public { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the playlist will be collaborative.
    /// </summary>
    [JsonPropertyName("collaborative")]
    public bool? Collaborative { get; set; }

    /// <summary>
    /// Gets or sets the new description for the playlist.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Represents the request payload used to add items to a playlist.
/// </summary>
public sealed class AddItemsToPlaylistRequest
{
    /// <summary>
    /// Gets or sets a list of Spotify URIs to add.
    /// </summary>
    [JsonPropertyName("uris")]
    public List<string>? Uris { get; set; }

    /// <summary>
    /// Gets or sets the zero-based index position where items should be inserted.
    /// </summary>
    [JsonPropertyName("position")]
    public int? Position { get; set; }
}

/// <summary>
/// Represents the request payload used to reorder or replace playlist items.
/// </summary>
public sealed class ReorderOrReplacePlaylistItemsRequest
{
    /// <summary>
    /// Gets or sets a list of Spotify URIs to replace playlist items with.
    /// </summary>
    [JsonPropertyName("uris")]
    public List<string>? Uris { get; set; }

    /// <summary>
    /// Gets or sets the position of the first item to reorder.
    /// </summary>
    [JsonPropertyName("range_start")]
    public int? RangeStart { get; set; }

    /// <summary>
    /// Gets or sets the position where the items should be inserted.
    /// </summary>
    [JsonPropertyName("insert_before")]
    public int? InsertBefore { get; set; }

    /// <summary>
    /// Gets or sets the number of items to reorder.
    /// </summary>
    [JsonPropertyName("range_length")]
    public int? RangeLength { get; set; }

    /// <summary>
    /// Gets or sets the playlist snapshot ID against which to apply the operations.
    /// </summary>
    [JsonPropertyName("snapshot_id")]
    public string? SnapshotId { get; set; }
}

/// <summary>
/// Represents the request payload used to delete specific items from a playlist.
/// </summary>
public sealed class RemovePlaylistItemsRequest
{
    /// <summary>
    /// Gets or sets the array of items to remove from the playlist.
    /// </summary>
    [JsonPropertyName("items")]
    public List<RemovePlaylistItemUnit> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the playlist snapshot ID against which the removal should occur.
    /// </summary>
    [JsonPropertyName("snapshot_id")]
    public string? SnapshotId { get; set; }
}

/// <summary>
/// Unit wrapper holding the identifier of a single playlist item targeting removal.
/// </summary>
public sealed class RemovePlaylistItemUnit
{
    /// <summary>
    /// Gets or sets the Spotify URI of the item to remove.
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = string.Empty;
}

/// <summary>
/// Represents the request payload used to create a new playlist.
/// </summary>
public sealed class CreatePlaylistRequest
{
    /// <summary>
    /// Gets or sets the name for the new playlist.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the new playlist should be public.
    /// </summary>
    [JsonPropertyName("public")]
    public bool? Public { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the new playlist should be collaborative.
    /// </summary>
    [JsonPropertyName("collaborative")]
    public bool? Collaborative { get; set; }

    /// <summary>
    /// Gets or sets the description for the new playlist.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Represents an album saved within the current user's personal library.
/// </summary>
public sealed class SpotifySavedAlbum
{
    /// <summary>
    /// Gets or sets the date and time when the album was saved to the user's library.
    /// </summary>
    [JsonPropertyName("added_at")]
    public DateTime AddedAt { get; set; }

    /// <summary>
    /// Gets or sets the album object metadata.
    /// </summary>
    [JsonPropertyName("album")]
    public SpotifyAlbum? Album { get; set; }
}

/// <summary>
/// Represents a track saved within the current user's personal library.
/// </summary>
public sealed class SpotifySavedTrack
{
    /// <summary>
    /// Gets or sets the date and time when the track was saved to the user's library.
    /// </summary>
    [JsonPropertyName("added_at")]
    public DateTime AddedAt { get; set; }

    /// <summary>
    /// Gets or sets the track object metadata.
    /// </summary>
    [JsonPropertyName("track")]
    public SpotifyTrack? Track { get; set; }
}

/// <summary>
/// Represents an episode saved within the current user's personal library.
/// </summary>
public sealed class SpotifySavedEpisode
{
    /// <summary>
    /// Gets or sets the date and time when the episode was saved to the user's library.
    /// </summary>
    [JsonPropertyName("added_at")]
    public DateTime AddedAt { get; set; }

    /// <summary>
    /// Gets or sets the episode object metadata.
    /// </summary>
    [JsonPropertyName("episode")]
    public SpotifyEpisode? Episode { get; set; }
}

/// <summary>
/// Represents a show saved within the current user's personal library.
/// </summary>
public sealed class SpotifySavedShow
{
    /// <summary>
    /// Gets or sets the date and time when the show was saved to the user's library.
    /// </summary>
    [JsonPropertyName("added_at")]
    public DateTime AddedAt { get; set; }

    /// <summary>
    /// Gets or sets the show object metadata.
    /// </summary>
    [JsonPropertyName("show")]
    public SpotifyShow? Show { get; set; }
}

public sealed class SpotifyDevice
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    [JsonPropertyName("is_private_session")]
    public bool IsPrivateSession { get; set; }

    [JsonPropertyName("is_restricted")]
    public bool IsRestricted { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string DeviceType { get; set; } = string.Empty;

    [JsonPropertyName("volume_percent")]
    public int VolumePercent { get; set; }

    [JsonPropertyName("supports_volume")]
    public bool SupportsVolume { get; set; }
}
