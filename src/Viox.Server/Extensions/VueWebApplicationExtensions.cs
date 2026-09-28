using Microsoft.Extensions.FileProviders;

public static class VueWebApplicationExtensions
{
    public static WebApplication UseVueApp(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Microsoft.AspNetCore.Builder.VueWebApplicationExtensions");

        var fullPath = Path.GetFullPath("/app/wwwroot/");

        if (!Directory.Exists(fullPath))
        {
            logger.LogWarning("Vue static file directory does not exist at path: {Path}", fullPath);
        }
        else
        {
            logger.LogInformation("Serving Vue app assets from path: {Path}", fullPath);
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

        return app;
    }
}
