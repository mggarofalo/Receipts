using Application.Models;

namespace Application.Interfaces.Services;

public record UserSummary(
	string Id,
	string Email,
	string? FirstName,
	string? LastName,
	IReadOnlyList<string> Roles,
	bool IsDisabled,
	DateTimeOffset CreatedAt,
	DateTimeOffset? LastLoginAt);

public interface IUserService
{
	Task<PagedResult<UserSummary>> ListUsersAsync(int offset, int limit, CancellationToken cancellationToken = default);
	Task<string?> FindUserIdByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
