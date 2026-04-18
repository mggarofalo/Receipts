namespace Infrastructure.Entities.Core;

public class AccountEntity
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public bool IsActive { get; set; }
}
