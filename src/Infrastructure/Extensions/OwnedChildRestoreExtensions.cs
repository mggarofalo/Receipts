using System.Linq.Expressions;
using System.Reflection;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class OwnedChildRestoreExtensions
{
	private static readonly MethodInfo RestoreChildrenOfTypeMethod =
		typeof(OwnedChildRestoreExtensions).GetMethod(nameof(RestoreChildrenOfType), BindingFlags.NonPublic | BindingFlags.Static)!;

	public static async Task RestoreOwnedChildrenAsync<TParent>(this ApplicationDbContext context, Guid parentId, CancellationToken cancellationToken)
		where TParent : class, ISoftDeletable
	{
		Type parentType = typeof(TParent);
		if (!OwnedChildrenMapProvider.Map.TryGetValue(parentType, out OwnedChildrenMapProvider.ParentEntry? parentEntry))
		{
			return;
		}

		foreach (OwnedChildrenMapProvider.OwnedChildEntry child in parentEntry.Children)
		{
			MethodInfo method = RestoreChildrenOfTypeMethod.MakeGenericMethod(child.ChildType);
			await (Task)method.Invoke(null, [context, parentId, child.FkPropertyName, cancellationToken])!;
		}
	}

	private static async Task RestoreChildrenOfType<TChild>(ApplicationDbContext context, Guid parentId, string fkPropertyName, CancellationToken cancellationToken)
		where TChild : class, ISoftDeletable
	{
		ParameterExpression param = Expression.Parameter(typeof(TChild), "e");
		MemberExpression fkAccess = Expression.Property(param, fkPropertyName);
		BinaryExpression fkEquals = Expression.Equal(fkAccess, Expression.Constant(parentId));
		Expression<Func<TChild, bool>> predicate = Expression.Lambda<Func<TChild, bool>>(fkEquals, param);

		List<TChild> items = await context.Set<TChild>()
			.IncludeDeleted()
			.Where(predicate)
			.Where(e => e.DeletedAt != null)
			.ToListAsync(cancellationToken);

		foreach (TChild item in items)
		{
			item.DeletedAt = null;
			item.DeletedByUserId = null;
			item.DeletedByApiKeyId = null;
		}
	}
}
