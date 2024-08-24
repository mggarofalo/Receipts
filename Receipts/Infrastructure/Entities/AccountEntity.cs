namespace Infrastructure.Entities;

public class AccountEntity
{
	public required Guid Id { get; set; }
	public required string AccountCode { get; set; }
	public required string Name { get; set; }
	public required bool IsActive { get; set; }
}
