using Viox.Core.Scheduler;
namespace Viox.Server.Services;

public interface IUserTaskSettingsFileReader
{
    Task<IReadOnlyList<UserTaskSettings>> ReadSettingsAsync(CancellationToken cancellationToken = default);
}
