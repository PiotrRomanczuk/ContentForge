using ContentForge.Application.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace ContentForge.Infrastructure.Services.Security;

// Uses ASP.NET Core Data Protection API for symmetric encryption.
// Data Protection = built-in .NET encryption framework. Keys are auto-rotated and stored securely.
// Like using crypto.createCipheriv() in Node.js but with automatic key management.
public class DataProtectionTokenEncryptionService : ITokenEncryptionService
{
    private readonly IDataProtector _protector;
    private readonly ILogger<DataProtectionTokenEncryptionService> _logger;

    public DataProtectionTokenEncryptionService(
        IDataProtectionProvider provider,
        ILogger<DataProtectionTokenEncryptionService> logger)
    {
        // CreateProtector("purpose") = creates a purpose-specific encryptor.
        // Different purposes produce different ciphertexts even with the same key.
        _protector = provider.CreateProtector("ContentForge.SocialAccount.AccessToken");
        _logger = logger;
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        try
        {
            return _protector.Unprotect(cipherText);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            // If decryption fails, the token was stored before encryption was enabled.
            // Return as-is for backward compatibility — it will be re-encrypted on next update.
            _logger.LogWarning("Failed to decrypt token — returning as plaintext for backward compatibility");
            return cipherText;
        }
    }
}
