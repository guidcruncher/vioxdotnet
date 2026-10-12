namespace Viox.Core.Scheduler;

public class UserTaskSettings
{

    public string Moment { get; set; } = "";

    public string Name { get; set; } = "";

    public string Command { get; set; } = "";

    public Dictionary<string, string> Parameters { get; set; } = new();

}

