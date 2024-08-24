namespace Domain;

public class Account
{
	public required Guid Id { get; set; } = Guid.NewGuid();
	public required string AccountCode { get; set; }
	public required string Name { get; set; }
	public required bool IsActive { get; set; }
}
