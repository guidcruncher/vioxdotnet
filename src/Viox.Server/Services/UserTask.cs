namespace Viox.Server.Services;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Viox.Core.Models;
using Viox.Core.Scheduler;
using Viox.Core.Services;
using Viox.Server.Abstraction;

public class UserTask : IScheduledTask
{
    public string Name => _name;
    public string Schedule => _schedule;
    public bool RunOnStartup => false;

    private string _name = String.Empty;
    private string _schedule = String.Empty;
    private string _moment = String.Empty;
    private UserTaskSettings _settings = new();
    private readonly IPlaybackQueueService _queue;
    private readonly IMediaPlayerControlSurface _mediaControl;
    private readonly MediaSourceResolverService _resolver;

    public UserTask(MediaSourceResolverService resolver, IPlaybackQueueService queue, IMediaPlayerControlSurface mediaControl)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _mediaControl = mediaControl ?? throw new ArgumentNullException(nameof(mediaControl));
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
    }

    public void Build(UserTaskSettings settings)
    {
        _name = settings.Name;
        _moment = settings.Moment;
        _schedule = _moment;
        _settings = settings;
    }

    public static IScheduledTask CreateUserTask(IServiceScope scope, UserTaskSettings settings)
    {
        UserTask task = ActivatorUtilities.CreateInstance<UserTask>(scope.ServiceProvider);
        task.Build(settings);
        return task as IScheduledTask;
    }

    public static IScheduledTask CreateUserTask(IHost host, UserTaskSettings settings)
    {
        UserTask task = ActivatorUtilities.CreateInstance<UserTask>(host.Services);
        task.Build(settings);
        return task as IScheduledTask;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        switch (_settings.Command.ToLower())
        {
            case "playtrack":
                if (_settings.Parameters.TryGetValue("Uri", out var uriValue))
                {
                    await PlayTrackAsync(uriValue, cancellationToken);
                }
                break;
        }
    }

    private async Task PlayTrackAsync(string rawUri, CancellationToken cancellationToken = default)
    {
        MediaUri? uri = MediaUriParser.ParseMediaUriValue(rawUri);
        if (uri is not null)
        {
            MediaMetaData? metaData = await _resolver.ResolveMetaData(uri, cancellationToken);
            if (metaData is not null)
            {
                await _queue.ClearAsync(cancellationToken);
                await _queue.EnqueueAsync(metaData, cancellationToken);
            }
        }
    }
}
