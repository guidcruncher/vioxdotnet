namespace Viox.Core.Scheduler;

using System.Threading;
using System.Threading.Tasks;

public interface IScheduledTask
{
    string Name { get; }
    string Schedule { get; }
    bool RunOnStartup { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}

