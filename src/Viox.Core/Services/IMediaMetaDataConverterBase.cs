// File: IMediaMetaDataConverterBase.cs
namespace Viox.Core.Services;

using Viox.Core.Models;

/// <summary>
/// Defines the contract for converting external media objects into standardized metadata models.
/// </summary>
public interface IMediaMetaDataConverterBase
{
    /// <summary>
    /// Gets the unique identifier name associated with this converter implementation.
    /// </summary>
    string Name => $"{Source}:{Type}";

    /// <summary>
    /// Gets the source origin identifier for the media converter.
    /// </summary>
    string Source { get; }

    /// <summary>
    /// Gets the target media type processed by the converter.
    /// </summary>
    string Type { get; }

    /// <summary>
    /// Converts an input object into standardized media metadata.
    /// </summary>
    /// <param name="input">The raw input object to convert.</param>
    /// <returns>A converted <see cref="MediaMetaData"/> instance.</returns>
    MediaMetaData Convert(object input);
}
