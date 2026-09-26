using Viox.Client.Spotify.Models;

namespace Viox.Client.Spotify.Services;

/// <summary>
/// Provides a simple helper to automatically fetch and accumulate paginated results.
/// </summary>
public class SpotifyPagingHelper
{

    public async Task<List<T>> FetchAllAsync<T>(
        Func<int, int, CancellationToken, Task<SpotifyPagedResult<T>>> fetchPageFunc, int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fetchPageFunc);

        List<T> accumulatedItems = [];
        int offset = 0;
        bool hasNextPage = true;

        while (hasNextPage)
        {
            cancellationToken.ThrowIfCancellationRequested();

            SpotifyPagedResult<T> result = await fetchPageFunc(offset, pageSize, cancellationToken);

            if (result.Items.Count == 0)
            {
                break;
            }

            accumulatedItems.AddRange(result.Items);

            offset += result.Items.Count;
            hasNextPage = !string.IsNullOrEmpty(result.Next) && offset < result.Total;
        }

        return accumulatedItems;
    }
}
