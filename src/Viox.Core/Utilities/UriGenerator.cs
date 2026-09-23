using System.Security.Cryptography;
using System.Text;

namespace Viox.Core.Utilities;

public class UriGenerator : IUriGenerator
{
    private const int NonceSize = 12; // 96 bits for AES-GCM
    private const int TagSize = 16;   // 128 bits for AES-GCM
    private readonly byte[] _key;

    public UriGenerator()
    {
        _key = Convert.FromBase64String("ajhYL3FQMnZLOUw0bU42ejdXMXRSN3lVM29QNXNBOGRGMGdIMmpLNGxNNk49");
        if (_key.Length != 32)
        {
            throw new ArgumentException("Key must be 256 bits (32 bytes).");
        }
    }

    public string Create(string plainText)
    {
        ArgumentNullException.ThrowIfNull(plainText);

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        byte[] cipherText = new byte[plainBytes.Length];
        byte[] tag = new byte[TagSize];

        using (var aesGcm = new AesGcm(_key, TagSize))
        {
            aesGcm.Encrypt(nonce, plainBytes, cipherText, tag);
        }

        // Combine Nonce + Tag + CipherText into a single payload
        byte[] combined = new byte[NonceSize + TagSize + cipherText.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, combined, NonceSize, TagSize);
        Buffer.BlockCopy(cipherText, 0, combined, NonceSize + TagSize, cipherText.Length);

        return Convert.ToBase64String(combined);
    }

    public string Decode(string protectedText)
    {
        ArgumentNullException.ThrowIfNull(protectedText);

        byte[] combined = Convert.FromBase64String(protectedText);

        if (combined.Length < NonceSize + TagSize)
        {
            throw new CryptographicException("Invalid ciphertext length.");
        }

        byte[] nonce = new byte[NonceSize];
        byte[] tag = new byte[TagSize];
        byte[] cipherText = new byte[combined.Length - NonceSize - TagSize];

        Buffer.BlockCopy(combined, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(combined, NonceSize, tag, 0, TagSize);
        Buffer.BlockCopy(combined, NonceSize + TagSize, cipherText, 0, cipherText.Length);

        byte[] plainBytes = new byte[cipherText.Length];

        using (var aesGcm = new AesGcm(_key, TagSize))
        {
            aesGcm.Decrypt(nonce, cipherText, tag, plainBytes);
        }

        return Encoding.UTF8.GetString(plainBytes);
    }
}
