namespace Viox.Core.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IEqPresetLoader
{
    Task<IReadOnlyDictionary<string, int[]>> LoadPresetsAsync(CancellationToken cancellationToken = default);

    Task<int[]?> GetPresetByNameAsync(string name, CancellationToken cancellationToken = default);
}
