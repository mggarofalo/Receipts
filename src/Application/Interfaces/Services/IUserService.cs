namespace Application.Interfaces.Services;

public record UserSummary(string Id, string Email, IReadOnlyList<string> Roles);

public record PagedUserList(
	IReadOnlyList<UserSummary> Items,
	int Page,
	int PageSize,
	int TotalCount);

public interface IUserService
{
	Task<PagedUserList> ListUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
