namespace Viox.Server.Abstraction;

using System;
using System.Threading;
using System.Threading.Tasks;

using Viox.Core.Models;

/// <summary>
/// Unified control surface for routing media operations to the appropriate underlying controller.
/// </summary>
public interface IMediaPlayerControlSurface
{
    MediaMetaData? GetCurrentTrack();
    Task PlayAsync(string uri, CancellationToken cancellationToken = default);
    Task PauseAsync(CancellationToken cancellationToken = default);
    Task ResumeAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default);
    Task NextAsync(CancellationToken cancellationToken = default);
    Task PreviousAsync(CancellationToken cancellationToken = default);
    Task SetVolumeAsync(int volumePercent, CancellationToken cancellationToken = default);
    Task<int> GetVolumeAsync(CancellationToken cancellationToken = default);
    Task<IMediaPlayerAdapter?> GetActivePlayerAsync(CancellationToken cancellationToken = default);
}

