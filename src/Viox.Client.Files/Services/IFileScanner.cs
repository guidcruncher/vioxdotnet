namespace Viox.Client.Files.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Viox.Core.Models;

public interface IFileScanner
{
    Task<IReadOnlyList<MediaMetaData>> ScanDirectoryAsync(
        string? overrideDirectoryPath = null,
        CancellationToken cancellationToken = default);
}

