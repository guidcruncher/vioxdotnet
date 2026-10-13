namespace Viox.Client.Librespot.Events;

public record MetadataEvent(
    string ContextUri,
    string Uri,
    string Name,
    List<string> ArtistNames,
    string AlbumName,
    string AlbumCoverUrl,
    long Position,
    long Duration,
    string Format,
    string Codec,
    int? Bitrate,
    int SampleRate,
    int? BitDepth
) : LibrespotEvent("metadata");
