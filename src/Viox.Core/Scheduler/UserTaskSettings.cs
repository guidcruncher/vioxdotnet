namespace Viox.Core.Scheduler;

public class UserTaskSettings
{
    public string Command { get; set; } = "";

    public Dictionary<string, string> Parameters { get; set; } = new();

}

