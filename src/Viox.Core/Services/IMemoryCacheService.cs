namespace Viox.Core.Services;

public interface IMemoryCacheService<TValue>
{
    ValueTask SetAsync(string key, TValue value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);

    ValueTask<TValue?> GetAsync(string key, CancellationToken cancellationToken = default);

    ValueTask<TValue?> GetOrCreateAsync(string key, Func<CancellationToken, ValueTask<TValue>> factory, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);

    ValueTask<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

    ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
