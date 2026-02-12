namespace Shared.ViewModels.Core;

public class ReceiptVM
{
	public Guid? Id { get; set; }
	public string? Description { get; set; }
	public string? Location { get; set; }
	public DateOnly? Date { get; set; }
	public decimal? TaxAmount { get; set; }
}
