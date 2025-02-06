using Application.Interfaces.Services;

namespace Infrastructure.Services;

public class ApiKeyValidatorService : IApiKeyValidatorService
{
    public bool ApiKeyIsValid(string apiKey)
    {
        // Implement your validation logic here
        // For example, you can check if the API key matches a predefined pattern or exists in a database
        // This is a simple example that checks if the API key is not null or empty
        return !string.IsNullOrEmpty(apiKey);
    }
}

