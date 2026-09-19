using Viox.Core.Models;

namespace Viox.Server.Models;

public class PlaybackState
{
    public string ActiveBackend { get; set; } = string.Empty;

    public MediaMetaData? Track { get; set; } = null;

    public double Position { get; set; } = 0;

    public bool Playing { get; set; } = false;

    public bool IsLive { get; set; } = false;
}

