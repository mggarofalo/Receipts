namespace Application.Interfaces.Services;

public interface IApiKeyValidatorService
{
	bool ApiKeyIsValid(string apiKey);
}
