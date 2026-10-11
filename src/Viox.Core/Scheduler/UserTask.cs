namespace Viox.Core.Scheduler;

using System.Threading;
using System.Threading.Tasks;

using Viox.Core.Utilities;

public class UserTask : IScheduledTask
{
    public string Name => _name;
    public string Schedule => _schedule;
    public bool RunOnStartup => false;

    private readonly string _name;
    private readonly string _schedule;
    private readonly DateTimeOffset _moment;
    private readonly UserTaskSettings _settings;

    public UserTask(string name, DateTimeOffset moment, UserTaskSettings settings)
    {
        _name = name;
        _moment = moment;
        _schedule = moment.ToCron();
        _settings = settings;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {

    }

}

