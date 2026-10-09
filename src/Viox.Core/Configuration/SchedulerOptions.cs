namespace Viox.Core.Scheduler;

public class SchedulerOptions
{
    public const string SectionName = "Scheduler";

    public bool Enabled { get; set; } = true;
    public int CheckIntervalSeconds { get; set; } = 15;
}

