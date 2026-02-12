namespace Shared.ViewModels.Core;

public class AccountVM
{
	public Guid? Id { get; set; }
	public string? AccountCode { get; set; }
	public string? Name { get; set; }
	public bool IsActive { get; set; }
}
