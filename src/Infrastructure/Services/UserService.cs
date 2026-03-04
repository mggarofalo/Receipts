using Application.Interfaces.Services;
using Application.Models;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class UserService(ApplicationDbContext dbContext) : IUserService
{
	public async Task<PagedResult<UserSummary>> ListUsersAsync(int offset, int limit, CancellationToken cancellationToken = default)
	{
		offset = Math.Max(0, offset);
		limit = Math.Clamp(limit, 1, 100);

		int totalCount = await dbContext.Users.CountAsync(cancellationToken);

		List<ApplicationUser> users = await dbContext.Users
			.OrderBy(u => u.Email)
			.Skip(offset)
			.Take(limit)
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
			user.FirstName,
			user.LastName,
			rolesByUserId.GetValueOrDefault(user.Id, []),
			user.LockoutEnabled && user.LockoutEnd > DateTimeOffset.UtcNow,
			user.CreatedAt,
			user.LastLoginAt
		)).ToList();

		return new PagedResult<UserSummary>(items, totalCount, offset, limit);
	}

	public async Task<string?> FindUserIdByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
	{
		return await dbContext.Users
			.Where(u => u.RefreshToken == refreshToken)
			.Select(u => u.Id)
			.FirstOrDefaultAsync(cancellationToken);
	}
}
