using System.Security.Cryptography;
using System.Text;

namespace Common;

public static class ApiKeyUtilities
{
    /// <summary>
    /// Generates a cryptographically secure API key.
    /// </summary>
    /// <param name="size">The number of random bytes to generate. A size of 32 (256 bits) is recommended.</param>
    /// <param name="urlSafe">If true, encodes the key using Base64 URL encoding.</param>
    /// <returns>A string representation of the generated API key.</returns>
    public static string GenerateApiKey(int size = 32, bool urlSafe = false)
    {
        byte[] randomBytes = new byte[size];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Hashes an API key using SHA256.
    /// </summary>
    /// <param name="apiKey">The API key to hash.</param>
    /// <returns>A string representation of the hashed API key.</returns>
    public static string HashApiKey(string apiKey)
    {
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Checks if an API key matches a hashed API key.
    /// </summary>
    /// <param name="apiKey">The API key to check.</param>
    /// <param name="hash">The hashed API key to compare against.</param>
    /// <returns>True if the API key, when hashed, matches the hashed key, false otherwise.</returns>
    public static bool ApiKeyMatchesHash(string apiKey, string hash)
    {
        return HashApiKey(apiKey) == hash;
    }
}
