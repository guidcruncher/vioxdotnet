// File: HashUtilities.cs
namespace Viox.Core.Utilities;

using System;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Provides utility methods for creating unique string hashes.
/// </summary>
public static class HashUtilities
{
    /// <summary>
    /// Generates a SHA-256 hexadecimal hash string for the input text.
    /// Recommended for general-purpose unique string identifiers where collision resistance is needed.
    /// </summary>
    /// <param name="input">The string to hash.</param>
    /// <returns>A lower-case 64-character hexadecimal SHA-256 string representation.</returns>
    public static string ToSha256Hex(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        ReadOnlySpan<byte> inputBytes = Encoding.UTF8.GetBytes(input);
        Span<byte> hashBytes = stackalloc byte[SHA256.HashSizeInBytes];

        SHA256.HashData(inputBytes, hashBytes);

        return Convert.ToHexStringLower(hashBytes);
    }

    /// <summary>
    /// Generates a fast 64-bit non-cryptographic hash (XxHash64) formatted as a hexadecimal string.
    /// Ideal for in-memory lookups, caching keys, or high-throughput non-security hash tasks.
    /// </summary>
    /// <param name="input">The string to hash.</param>
    /// <returns>A lower-case 16-character hexadecimal XxHash64 representation.</returns>
    public static string ToXxHash64Hex(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        ReadOnlySpan<byte> inputBytes = Encoding.UTF8.GetBytes(input);
        Span<byte> hashBytes = stackalloc byte[sizeof(ulong)];

        XxHash64.Hash(inputBytes, hashBytes);

        return Convert.ToHexStringLower(hashBytes);
    }

    /// <summary>
    /// Generates a Base64 encoded SHA-256 string for compact string representations.
    /// </summary>
    /// <param name="input">The string to hash.</param>
    /// <returns>A URL-safe Base64 encoded hash string.</returns>
    public static string ToSha256Base64(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        ReadOnlySpan<byte> inputBytes = Encoding.UTF8.GetBytes(input);
        Span<byte> hashBytes = stackalloc byte[SHA256.HashSizeInBytes];

        SHA256.HashData(inputBytes, hashBytes);

        return Convert.ToBase64String(hashBytes);
    }
}
