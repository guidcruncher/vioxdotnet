using Viox.Server.Models;

namespace Viox.Server.Services;

public interface IClientOptionsStore
{
    Task<ClientConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
    Task SaveConfigurationAsync(ClientConfiguration configuration, CancellationToken cancellationToken = default);
}
