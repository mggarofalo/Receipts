using Application.Interfaces.Services;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class UserService(ApplicationDbContext dbContext) : IUserService
{
	public async Task<PagedUserList> ListUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default)
	{
		page = Math.Max(1, page);
		pageSize = Math.Clamp(pageSize, 1, 100);

		int totalCount = await dbContext.Users.CountAsync(cancellationToken);

		List<ApplicationUser> users = await dbContext.Users
			.OrderBy(u => u.Email)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		List<string> userIds = users.Select(u => u.Id).ToList();

		Dictionary<string, List<string>> rolesByUserId = await dbContext.UserRoles
			.Where(ur => userIds.Contains(ur.UserId))
			.Join(
				dbContext.Roles,
				ur => ur.RoleId,
				r => r.Id,
				(ur, r) => new { ur.UserId, RoleName = r.Name! })
			.GroupBy(x => x.UserId)
			.ToDictionaryAsync(
				g => g.Key,
				g => g.Select(x => x.RoleName).ToList(),
				cancellationToken);

		List<UserSummary> items = users.Select(user => new UserSummary(
			user.Id,
			user.Email ?? "",
			rolesByUserId.GetValueOrDefault(user.Id, [])
		)).ToList();

		return new PagedUserList(items, page, pageSize, totalCount);
	}
}
