namespace Infrastructure.Entities;

public class AccountEntity
{
	public Guid Id { get; set; }
	public string AccountCode { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public bool IsActive { get; set; }
}
