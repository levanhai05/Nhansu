using System.Security.Cryptography;
using System.Text;

namespace QLNhanSu.Services;

// Version 1: PBKDF2-HMAC-SHA256, 600,000 iterations, 32-byte salt and key.
// Legacy hashes have 64 bytes; new hashes have a version byte + 32-byte key.
public static class PasswordHashing
{
    public static void Create(string password, out byte[] hash, out byte[] salt)
    {
        salt = RandomNumberGenerator.GetBytes(32);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, 600_000, HashAlgorithmName.SHA256, 32);
        hash = new byte[33];
        hash[0] = 1;
        key.CopyTo(hash, 1);
    }

    public static bool Verify(string password, byte[] hash, byte[] salt, out bool needsUpgrade)
    {
        needsUpgrade = false;
        if (hash.Length == 33 && hash[0] == 1 && salt.Length == 32)
        {
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, 600_000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(key, hash.AsSpan(1));
        }
        if (hash.Length != 64 || salt.Length == 0) return false;
        using var hmac = new HMACSHA512(salt);
        var legacy = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        needsUpgrade = CryptographicOperations.FixedTimeEquals(legacy, hash);
        return needsUpgrade;
    }
}
