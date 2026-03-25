namespace ContentForge.Application.Services;

// Encrypts/decrypts sensitive tokens before storing in the database.
// Like using crypto.createCipher/Decipher in Node.js for at-rest encryption.
public interface ITokenEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
