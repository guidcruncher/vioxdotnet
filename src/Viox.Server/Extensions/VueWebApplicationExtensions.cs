using Microsoft.Extensions.FileProviders;

public static class VueWebApplicationExtensions
{
    public static WebApplication UseVueApp(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        string disableWebUi = Environment.GetEnvironmentVariable("DISABLE_AUDIOHUB_WEBUI") ?? "false";

        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Microsoft.AspNetCore.Builder.VueWebApplicationExtensions");

        if (disableWebUi.ToLower() == "true")
        {
            logger.LogWarning("Built-in AudioHub WebUI is disabled.");
            return app;
        }

        var fullPath = Path.GetFullPath("/app/wwwroot/");

        if (!Directory.Exists(fullPath))
        {
            logger.LogWarning("Built-in AudioHub WebUI files not found at path {'Path}' Cannot serve WebUI", fullPath);
            return app;
        }
        else
        {
            logger.LogInformation("Serving Built-in AudioHub WebUI from path '{Path}'", fullPath);
        }

        var fileProvider = new PhysicalFileProvider(fullPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = fileProvider,
            RequestPath = string.Empty
        });

        app.MapFallbackToFile("index.html", new StaticFileOptions
        {
            FileProvider = fileProvider
        });

        logger.LogInformation("Built-in AudioHub WebUI setup and being served.");
        return app;
    }
}
