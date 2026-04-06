namespace Infrastructure.Interfaces;

/// <summary>
/// Overrides the FK property name convention used by <see cref="IOwnedBy{TParent}"/>.
/// By default, the FK property name is derived as "{ParentNameWithoutEntity}Id".
/// Apply this attribute when the child's FK property does not follow that convention.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class OwnedByFkAttribute(Type parentType, string fkPropertyName) : Attribute
{
	public Type ParentType { get; } = parentType;
	public string FkPropertyName { get; } = fkPropertyName;
}
