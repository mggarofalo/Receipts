using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Reflection;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class OwnedChildRestoreExtensions
{
	private static readonly FrozenDictionary<Type, List<(Type ChildType, string FkPropertyName)>> OwnedChildrenMap = BuildOwnedChildrenMap();

	private static FrozenDictionary<Type, List<(Type ChildType, string FkPropertyName)>> BuildOwnedChildrenMap()
	{
		Dictionary<Type, List<(Type, string)>> map = [];
		Assembly assembly = typeof(OwnedChildRestoreExtensions).Assembly;

		foreach (Type type in assembly.GetTypes())
		{
			if (type.IsAbstract || type.IsInterface || !typeof(ISoftDeletable).IsAssignableFrom(type))
			{
				continue;
			}

			foreach (Type iface in type.GetInterfaces())
			{
				if (!iface.IsGenericType || iface.GetGenericTypeDefinition() != typeof(IOwnedBy<>))
				{
					continue;
				}

				Type parentType = iface.GetGenericArguments()[0];
				string parentName = parentType.Name.Replace("Entity", "");
				string fkPropertyName = $"{parentName}Id";

				if (!map.TryGetValue(parentType, out List<(Type, string)>? children))
				{
					children = [];
					map[parentType] = children;
				}

				children.Add((type, fkPropertyName));
			}
		}

		return map.ToFrozenDictionary();
	}

	private static readonly MethodInfo RestoreChildrenOfTypeMethod =
		typeof(OwnedChildRestoreExtensions).GetMethod(nameof(RestoreChildrenOfType), BindingFlags.NonPublic | BindingFlags.Static)!;

	public static async Task RestoreOwnedChildrenAsync<TParent>(this ApplicationDbContext context, Guid parentId, CancellationToken cancellationToken)
		where TParent : class, ISoftDeletable
	{
		Type parentType = typeof(TParent);
		if (!OwnedChildrenMap.TryGetValue(parentType, out List<(Type ChildType, string FkPropertyName)>? children))
		{
			return;
		}

		foreach ((Type childType, string fkPropertyName) in children)
		{
			MethodInfo method = RestoreChildrenOfTypeMethod.MakeGenericMethod(childType);
			await (Task)method.Invoke(null, [context, parentId, fkPropertyName, cancellationToken])!;
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
