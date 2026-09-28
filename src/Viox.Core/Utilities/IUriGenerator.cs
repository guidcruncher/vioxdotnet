namespace Viox.Core.Utilities;

/// <summary>
/// Defines a contract for creating and decoding protected, URI-safe text strings.
/// </summary>
public interface IUriGenerator
{
    /// <summary>
    /// Transforms plain text into a protected, URI-safe string representation.
    /// </summary>
    /// <param name="plainText">The unencrypted or unencoded string to protect.</param>
    /// <returns>A protected, URI-safe string suitable for inclusion in URIs.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="plainText"/> is <see langword="null"/>.
    /// </exception>
    string Create(string plainText);

    /// <summary>
    /// Decodes and restores a previously protected, URI-safe string back to its original plain text.
    /// </summary>
    /// <param name="protectedText">The protected, URI-safe string to decode.</param>
    /// <returns>The original unencoded plain text string.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="protectedText"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="System.FormatException">
    /// Thrown when <paramref name="protectedText"/> is not a valid or well-formed protected string.
    /// </exception>
    string Decode(string protectedText);
}
