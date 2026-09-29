namespace Viox.Client.RadioBrowser.Services;

public class RadioBrowserInitializer
{
    public string? FastestUrl { get; private set; }

    public async Task InitializeAsync()
    {
        FastestUrl = await RadioBrowserApiServerResolver.GetFastestApiUrlAsync();
    }
}
