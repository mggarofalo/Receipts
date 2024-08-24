namespace Shared.ViewModels;

public class AccountVM
{
	public Guid? Id { get; set; }
	public required string AccountCode { get; set; }
	public required string Name { get; set; }
	public required bool IsActive { get; set; }
}
