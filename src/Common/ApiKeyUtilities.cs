using System.Security.Cryptography;

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
}
