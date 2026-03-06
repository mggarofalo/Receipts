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

}
