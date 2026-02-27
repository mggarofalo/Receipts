namespace Application.Exceptions;

/// <summary>
/// Thrown when an operation would create a duplicate entity (e.g., unique constraint violation).
/// Abstracts persistence-layer exceptions so Presentation never references EF Core types.
/// </summary>
public class DuplicateEntityException : Exception
{
	public DuplicateEntityException(string message) : base(message) { }

	public DuplicateEntityException(string message, Exception innerException) : base(message, innerException) { }
}
