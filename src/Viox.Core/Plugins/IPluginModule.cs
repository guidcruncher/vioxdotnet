namespace Viox.Core.Plugins;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public interface IPluginModule
{
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}
