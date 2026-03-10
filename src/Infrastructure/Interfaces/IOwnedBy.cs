namespace Infrastructure.Interfaces;

/// <summary>
/// Marker interface declaring that an entity is owned by a parent entity.
/// The FK property is discovered by convention: a property named "{TParent.Name without 'Entity'}Id"
/// (e.g., ReceiptId for IOwnedBy&lt;ReceiptEntity&gt;).
/// When the parent is soft-deleted, owned children are cascade soft-deleted.
/// When the parent is restored, owned children are cascade restored.
/// </summary>
public interface IOwnedBy<TParent> where TParent : class, ISoftDeletable
{
}
