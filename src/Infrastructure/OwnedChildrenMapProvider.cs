using System.Collections.Frozen;
using System.Reflection;
using Infrastructure.Interfaces;

namespace Infrastructure;

/// <summary>
/// Single source of truth for the parent → owned-children map discovered
/// via <see cref="IOwnedBy{TParent}"/> marker interfaces.  Both
/// <see cref="ApplicationDbContext"/> (cascade soft-delete) and
/// <see cref="Extensions.OwnedChildRestoreExtensions"/> (cascade restore)
/// consume this map so the reflection work is done exactly once.
/// </summary>
internal static class OwnedChildrenMapProvider
{
	internal sealed record OwnedChildEntry(Type ChildType, string FkPropertyName, PropertyInfo FkProperty);

	internal sealed record ParentEntry(PropertyInfo IdProperty, List<OwnedChildEntry> Children);

	internal static readonly FrozenDictionary<Type, ParentEntry> Map = BuildMap();

	private static FrozenDictionary<Type, ParentEntry> BuildMap()
	{
		Dictionary<Type, List<OwnedChildEntry>> childMap = [];
		Assembly assembly = typeof(OwnedChildrenMapProvider).Assembly;

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

				PropertyInfo fkProperty = type.GetProperty(fkPropertyName)
					?? throw new InvalidOperationException(
						$"{type.Name} implements IOwnedBy<{parentType.Name}> but has no property '{fkPropertyName}'. "
						+ $"Add a '{fkPropertyName}' property or rename the FK to match the convention.");

				if (!childMap.TryGetValue(parentType, out List<OwnedChildEntry>? children))
				{
					children = [];
					childMap[parentType] = children;
				}

				children.Add(new OwnedChildEntry(type, fkPropertyName, fkProperty));
			}
		}

		Dictionary<Type, ParentEntry> result = [];
		foreach ((Type parentType, List<OwnedChildEntry> children) in childMap)
		{
			PropertyInfo idProperty = parentType.GetProperty("Id")
				?? throw new InvalidOperationException(
					$"{parentType.Name} is used as a parent in IOwnedBy<> but has no 'Id' property.");

			result[parentType] = new ParentEntry(idProperty, children);
		}

		return result.ToFrozenDictionary();
	}
}
