using Common;

namespace Common.Tests;

public class ApiKeyUtilitiesTests
{
	[Fact]
	public void GenerateApiKey_ReturnsBase64String()
	{
		// Act
		string key = ApiKeyUtilities.GenerateApiKey();

		// Assert
		Assert.False(string.IsNullOrWhiteSpace(key));
		byte[] decoded = Convert.FromBase64String(key);
		Assert.Equal(32, decoded.Length);
	}

	[Fact]
	public void GenerateApiKey_RespectsCustomSize()
	{
		// Act
		string key = ApiKeyUtilities.GenerateApiKey(size: 16);

		// Assert
		byte[] decoded = Convert.FromBase64String(key);
		Assert.Equal(16, decoded.Length);
	}

	[Fact]
	public void GenerateApiKey_ProducesDifferentKeysEachCall()
	{
		// Act
		string key1 = ApiKeyUtilities.GenerateApiKey();
		string key2 = ApiKeyUtilities.GenerateApiKey();

		// Assert
		Assert.NotEqual(key1, key2);
	}

	[Fact]
	public void HashApiKey_ReturnsDeterministicHash()
	{
		// Arrange
		string apiKey = "test-api-key";

		// Act
		string hash1 = ApiKeyUtilities.HashApiKey(apiKey);
		string hash2 = ApiKeyUtilities.HashApiKey(apiKey);

		// Assert
		Assert.Equal(hash1, hash2);
	}

	[Fact]
	public void HashApiKey_DifferentKeysProduceDifferentHashes()
	{
		// Act
		string hash1 = ApiKeyUtilities.HashApiKey("key-1");
		string hash2 = ApiKeyUtilities.HashApiKey("key-2");

		// Assert
		Assert.NotEqual(hash1, hash2);
	}

	[Fact]
	public void ApiKeyMatchesHash_ReturnsTrueForMatchingKey()
	{
		// Arrange
		string apiKey = ApiKeyUtilities.GenerateApiKey();
		string hash = ApiKeyUtilities.HashApiKey(apiKey);

		// Act & Assert
		Assert.True(ApiKeyUtilities.ApiKeyMatchesHash(apiKey, hash));
	}

	[Fact]
	public void ApiKeyMatchesHash_ReturnsFalseForWrongKey()
	{
		// Arrange
		string apiKey = ApiKeyUtilities.GenerateApiKey();
		string hash = ApiKeyUtilities.HashApiKey(apiKey);

		// Act & Assert
		Assert.False(ApiKeyUtilities.ApiKeyMatchesHash("wrong-key", hash));
	}
}
