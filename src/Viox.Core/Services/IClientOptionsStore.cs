using Viox.Core.Models;

namespace Viox.Core.Services;

public interface IClientOptionsStore
{
    Task<ClientConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
    Task SaveConfigurationAsync(ClientConfiguration configuration, CancellationToken cancellationToken = default);
}
