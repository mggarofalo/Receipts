namespace Application.Interfaces.Services;

public interface ITokenService
{
	string GenerateAccessToken(string userId, string email, IList<string> roles);
	string GenerateRefreshToken();
}
