namespace Domain.NormalizedDescriptions;

public class NormalizedDescription
{
	public Guid Id { get; set; }
	public string CanonicalName { get; set; }
	public NormalizedDescriptionStatus Status { get; set; }
	public DateTimeOffset CreatedAt { get; set; }

	public const string CanonicalNameCannotBeEmpty = "Canonical name cannot be empty";

	public NormalizedDescription(Guid id, string canonicalName, NormalizedDescriptionStatus status, DateTimeOffset createdAt)
	{
		if (string.IsNullOrWhiteSpace(canonicalName))
		{
			throw new ArgumentException(CanonicalNameCannotBeEmpty, nameof(canonicalName));
		}

		Id = id;
		CanonicalName = canonicalName;
		Status = status;
		CreatedAt = createdAt;
	}
}
