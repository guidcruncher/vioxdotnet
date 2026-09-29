namespace Viox.Core.Plugins;

using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public interface IPluginModule
{
    Task ConfigureServices(IServiceCollection services, IConfiguration configuration);
}
