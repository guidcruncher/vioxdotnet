namespace Viox.Client.RadioBrowser.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Viox.Client.RadioBrowser.Configuration;

public class RadioBrowserInitializerHostedService : IHostedService
{
    private readonly RadioBrowserInitializer initializer;
    private readonly RadioBrowserOptions options;

    public RadioBrowserInitializerHostedService(
        RadioBrowserInitializer initializer,
        IOptions<RadioBrowserOptions> clientOptions)
    {
        this.initializer = initializer;
        this.options = clientOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await initializer.InitializeAsync();
        options.BaseAddress = new Uri($"https://{initializer.FastestUrl}");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
