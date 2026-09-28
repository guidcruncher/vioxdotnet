using System.Text.Json.Serialization;

namespace Viox.Client.TuneIn.Models;

/// <summary>
/// Represents the root response envelope from the TuneIn OPML JSON API.
/// </summary>
public sealed class TuneInResponse<T>
{
    /// <summary>
    /// Top-level response head containing status code and title metadata.
    /// </summary>
    [JsonPropertyName("head")]
    public TuneInHeader? Head { get; set; }

    /// <summary>
    /// List of outlines returned by the operation.
    /// </summary>
    [JsonPropertyName("body")]
    public List<T>? Body { get; set; }
}

/// <summary>
/// Response header containing execution status and title.
/// </summary>
public sealed class TuneInHeader
{
    /// <summary>
    /// Title of the current response category or payload.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// HTTP status code returned by the backend endpoint (e.g., 200).
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

/// <summary>
/// Represents an entry, container, station, show, or audio stream in TuneIn API payloads.
/// </summary>
public sealed class TuneInOutline
{
    /// <summary>
    /// Type of the item (e.g., "link", "audio", "container").
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("key")]
    public string? Key { get; set; }

    /// <summary>
    /// Text label or title of the element.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Target URL for browsing child categories or requesting tune endpoints.
    /// </summary>
    [JsonPropertyName("URL")]
    public string? Url { get; set; }

    /// <summary>
    /// Bitrate of the media stream in kbps, if applicable.
    /// </summary>
    [JsonPropertyName("bitrate")]
    public string? Bitrate { get; set; }

    /// <summary>
    /// Reliability score indicator for the station.
    /// </summary>
    [JsonPropertyName("reliability")]
    public string? Reliability { get; set; }

    /// <summary>
    /// Uri of item
    /// </summary>
    public string? Uri { get => (Type == "audio" ? $"tunein:station:{GuideId}" : null); }

    /// <summary>
    /// Guide ID uniquely identifying the station, podcast, topic, or container.
    /// </summary>
    [JsonPropertyName("guide_id")]
    public string? GuideId { get; set; }

    /// <summary>
    /// Subtext or secondary summary line.
    /// </summary>
    [JsonPropertyName("subtext")]
    public string? Subtext { get; set; }

    /// <summary>
    /// Associated genre identifier.
    /// </summary>
    [JsonPropertyName("genre_id")]
    public string? GenreId { get; set; }

    /// <summary>
    /// Supported encoding formats (e.g., "mp3", "aac").
    /// </summary>
    [JsonPropertyName("formats")]
    public string? Formats { get; set; }

    /// <summary>
    /// Current track, show, or song currently playing.
    /// </summary>
    [JsonPropertyName("playing")]
    public string? Playing { get; set; }

    /// <summary>
    /// Image or thumbnail URL associated with the entity.
    /// </summary>
    [JsonPropertyName("image")]
    public string? Image { get; set; }

    /// <summary>
    /// Item classification type (e.g., "station", "show", "topic").
    /// </summary>
    [JsonPropertyName("item")]
    public string? Item { get; set; }

    /// <summary>
    /// Child outlines contained within a folder or category response.
    /// </summary>
    [JsonPropertyName("children")]
    public List<TuneInOutline>? Children { get; set; }
}

/// <summary>
/// Represents media stream element metadata from an audio JSON payload.
/// </summary>
public sealed class AudioElement
{
    /// <summary>
    /// Gets or sets the element type (e.g., "audio").
    /// </summary>
    [JsonPropertyName("element")]
    public string Element { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of the media stream.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stream reliability metric.
    /// </summary>
    [JsonPropertyName("reliability")]
    public int Reliability { get; set; }

    /// <summary>
    /// Gets or sets the stream bitrate in kbps.
    /// </summary>
    [JsonPropertyName("bitrate")]
    public int Bitrate { get; set; }

    /// <summary>
    /// Gets or sets the media format type (e.g., "mp3").
    /// </summary>
    [JsonPropertyName("media_type")]
    public string MediaType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the position index.
    /// </summary>
    [JsonPropertyName("position")]
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets the target player width in pixels.
    /// </summary>
    [JsonPropertyName("player_width")]
    public int PlayerWidth { get; set; }

    /// <summary>
    /// Gets or sets the target player height in pixels.
    /// </summary>
    [JsonPropertyName("player_height")]
    public int PlayerHeight { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether HLS advanced streaming is enabled.
    /// Represented as a string boolean in JSON ("true"/"false").
    /// </summary>
    [JsonPropertyName("is_hls_advanced")]
    public string IsHlsAdvanced { get; set; } = "false";

    /// <summary>
    /// Gets or sets a value indicating whether live seek stream is enabled.
    /// Represented as a string boolean in JSON ("true"/"false").
    /// </summary>
    [JsonPropertyName("live_seek_stream")]
    public string LiveSeekStream { get; set; } = "false";

    /// <summary>
    /// Gets or sets the unique guide identifier.
    /// </summary>
    [JsonPropertyName("guide_id")]
    public string GuideId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether ad-clipped content is enabled.
    /// Represented as a string boolean in JSON ("true"/"false").
    /// </summary>
    [JsonPropertyName("is_ad_clipped_content_enabled")]
    public string IsAdClippedContentEnabled { get; set; } = "false";

    /// <summary>
    /// Gets or sets a value indicating whether the stream connection is direct.
    /// </summary>
    [JsonPropertyName("is_direct")]
    public bool IsDirect { get; set; }
}

/// <summary>
/// Represents station metadata returned from the TuneIn API.
/// </summary>
public sealed class StationElement
{
    /// <summary>
    /// Gets the Uri
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get => $"tunein:station:{GuideId}"; }

    /// <summary>
    /// Gets or sets the element type (e.g., "station").
    /// </summary>
    [JsonPropertyName("element")]
    public string Element { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the guide identifier.
    /// </summary>
    [JsonPropertyName("guide_id")]
    public string GuideId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the preset identifier.
    /// </summary>
    [JsonPropertyName("preset_id")]
    public string PresetId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the station.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the call sign.
    /// </summary>
    [JsonPropertyName("call_sign")]
    public string CallSign { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the station slogan.
    /// </summary>
    [JsonPropertyName("slogan")]
    public string Slogan { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the broadcast frequency, if applicable.
    /// </summary>
    [JsonPropertyName("frequency")]
    public string Frequency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the broadcast band (e.g., "DAB", "FM").
    /// </summary>
    [JsonPropertyName("band")]
    public string Band { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the official station URL.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reporting URL.
    /// </summary>
    [JsonPropertyName("report_url")]
    public string ReportUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detail URL.
    /// </summary>
    [JsonPropertyName("detail_url")]
    public string DetailUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the station is a preset.
    /// </summary>
    [JsonPropertyName("is_preset")]
    public bool IsPreset { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the station is currently available.
    /// </summary>
    [JsonPropertyName("is_available")]
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the station primarily plays music.
    /// </summary>
    [JsonPropertyName("is_music")]
    public bool IsMusic { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the station provides active song information.
    /// </summary>
    [JsonPropertyName("has_song")]
    public bool HasSong { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether schedule information is available.
    /// </summary>
    [JsonPropertyName("has_schedule")]
    public bool HasSchedule { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether topic info is available.
    /// </summary>
    [JsonPropertyName("has_topics")]
    public bool HasTopics { get; set; }

    /// <summary>
    /// Gets or sets the Twitter handle.
    /// </summary>
    [JsonPropertyName("twitter_id")]
    public string TwitterId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL for the station logo.
    /// </summary>
    [JsonPropertyName("logo")]
    public string Logo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the station location string.
    /// </summary>
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the current playing song.
    /// </summary>
    [JsonPropertyName("current_song")]
    public string? CurrentSong { get; set; }

    /// <summary>
    /// Gets or sets the current artist name.
    /// </summary>
    [JsonPropertyName("current_artist")]
    public string? CurrentArtist { get; set; }

    /// <summary>
    /// Gets or sets the current artist identifier.
    /// </summary>
    [JsonPropertyName("current_artist_id")]
    public string? CurrentArtistId { get; set; }

    /// <summary>
    /// Gets or sets the current album name.
    /// </summary>
    [JsonPropertyName("current_album")]
    public string? CurrentAlbum { get; set; }

    /// <summary>
    /// Gets or sets the current artist artwork URL.
    /// </summary>
    [JsonPropertyName("current_artist_art")]
    public string? CurrentArtistArt { get; set; }

    /// <summary>
    /// Gets or sets the current album artwork URL.
    /// </summary>
    [JsonPropertyName("current_album_art")]
    public string? CurrentAlbumArt { get; set; }

    /// <summary>
    /// Gets or sets the station description.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the station contact email.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the station phone number.
    /// </summary>
    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mailing address.
    /// </summary>
    [JsonPropertyName("mailing_address")]
    public string MailingAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary broadcast language.
    /// </summary>
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary genre identifier.
    /// </summary>
    [JsonPropertyName("genre_id")]
    public string GenreId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary genre name.
    /// </summary>
    [JsonPropertyName("genre_name")]
    public string GenreName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the region identifier.
    /// </summary>
    [JsonPropertyName("region_id")]
    public int? RegionId { get; set; }

    /// <summary>
    /// Gets or sets the country region identifier.
    /// </summary>
    [JsonPropertyName("country_region_id")]
    public int? CountryRegionId { get; set; }

    /// <summary>
    /// Gets or sets the latitude and longitude string representation.
    /// </summary>
    [JsonPropertyName("latlon")]
    public string? LatLon { get; set; }

    /// <summary>
    /// Gets or sets the station timezone string.
    /// </summary>
    [JsonPropertyName("tz")]
    public string Tz { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timezone offset in minutes (represented as a string in the API payload).
    /// </summary>
    [JsonPropertyName("tz_offset")]
    public string TzOffset { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the published song information.
    /// </summary>
    [JsonPropertyName("publish_song")]
    public string? PublishSong { get; set; }

    /// <summary>
    /// Gets or sets the published song URL.
    /// </summary>
    [JsonPropertyName("publish_song_url")]
    public string? PublishSongUrl { get; set; }

    /// <summary>
    /// Gets or sets the rejection reason when a published song fails.
    /// </summary>
    [JsonPropertyName("publish_song_rejection_reason")]
    public string? PublishSongRejectionReason { get; set; }

    /// <summary>
    /// Gets or sets the URL to retrieve now playing data.
    /// </summary>
    [JsonPropertyName("now_playing_url")]
    public string? NowPlayingUrl { get; set; }

    /// <summary>
    /// Gets or sets the external identifier key.
    /// </summary>
    [JsonPropertyName("external_key")]
    public string? ExternalKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether ads are eligible.
    /// </summary>
    [JsonPropertyName("ad_eligible")]
    public bool AdEligible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether pre-roll ads are eligible.
    /// </summary>
    [JsonPropertyName("preroll_ad_eligible")]
    public bool PrerollAdEligible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether companion ads are eligible.
    /// </summary>
    [JsonPropertyName("companion_ad_eligible")]
    public bool CompanionAdEligible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether video pre-roll ads are eligible.
    /// </summary>
    [JsonPropertyName("video_preroll_ad_eligible")]
    public bool VideoPrerollAdEligible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Facebook sharing is available.
    /// </summary>
    [JsonPropertyName("fb_share")]
    public bool FbShare { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Twitter sharing is available.
    /// </summary>
    [JsonPropertyName("twitter_share")]
    public bool TwitterShare { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether song sharing is available.
    /// </summary>
    [JsonPropertyName("song_share")]
    public bool SongShare { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether donations are eligible (returned as string "true"/"false").
    /// </summary>
    [JsonPropertyName("donation_eligible")]
    public string DonationEligible { get; set; } = "false";

    /// <summary>
    /// Gets or sets the donation URL.
    /// </summary>
    [JsonPropertyName("donation_url")]
    public string? DonationUrl { get; set; }

    /// <summary>
    /// Gets or sets the donation callout text.
    /// </summary>
    [JsonPropertyName("donation_text")]
    public string? DonationText { get; set; }

    /// <summary>
    /// Gets or sets the icon URL associated with donations.
    /// </summary>
    [JsonPropertyName("donation_icon")]
    public string? DonationIcon { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether song purchasing is eligible.
    /// </summary>
    [JsonPropertyName("song_buy_eligible")]
    public bool? SongBuyEligible { get; set; }

    /// <summary>
    /// Gets or sets the direct TuneIn landing page URL for the station.
    /// </summary>
    [JsonPropertyName("tunein_url")]
    public string TuneInUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the station is suitable for families.
    /// </summary>
    [JsonPropertyName("is_family_content")]
    public bool IsFamilyContent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the content is tagged as mature.
    /// </summary>
    [JsonPropertyName("is_mature_content")]
    public bool IsMatureContent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this entry represents a specific live event.
    /// </summary>
    [JsonPropertyName("is_event")]
    public bool IsEvent { get; set; }

    /// <summary>
    /// Gets or sets the content classification tag (e.g., "talk").
    /// </summary>
    [JsonPropertyName("content_classification")]
    public string ContentClassification { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total favorited count.
    /// </summary>
    [JsonPropertyName("favorited_count")]
    public int? FavoritedCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has favorited this station.
    /// </summary>
    [JsonPropertyName("is_favorited")]
    public bool? IsFavorited { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the station can be favorited.
    /// </summary>
    [JsonPropertyName("is_favoritable")]
    public bool? IsFavoritable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the station has a profile page (returned as string "true"/"false").
    /// </summary>
    [JsonPropertyName("has_profile")]
    public string HasProfile { get; set; } = "false";

    /// <summary>
    /// Gets or sets a value indicating whether the stream can be cast to external devices.
    /// </summary>
    [JsonPropertyName("can_cast")]
    public bool CanCast { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Nielsen tracking applies.
    /// </summary>
    [JsonPropertyName("nielsen_eligible")]
    public bool NielsenEligible { get; set; }

    /// <summary>
    /// Gets or sets the Nielsen provider name.
    /// </summary>
    [JsonPropertyName("nielsen_provider")]
    public string? NielsenProvider { get; set; }

    /// <summary>
    /// Gets or sets the Nielsen asset identifier.
    /// </summary>
    [JsonPropertyName("nielsen_asset_id")]
    public string? NielsenAssetId { get; set; }

    /// <summary>
    /// Gets or sets the now playing metadata channel.
    /// </summary>
    [JsonPropertyName("nowplaying_channel")]
    public string? NowPlayingChannel { get; set; }

    /// <summary>
    /// Gets or sets the descriptive text explaining advertisement placement.
    /// </summary>
    [JsonPropertyName("why_ads_text")]
    public string? WhyAdsText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to force the native device media player.
    /// </summary>
    [JsonPropertyName("use_native_player")]
    public bool UseNativePlayer { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether live seek stream is supported.
    /// </summary>
    [JsonPropertyName("live_seek_stream")]
    public bool LiveSeekStream { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether seeking is disabled for this stream.
    /// </summary>
    [JsonPropertyName("seek_disabled")]
    public bool SeekDisabled { get; set; }
}
