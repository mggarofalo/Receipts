namespace Infrastructure.Entities.Core;

public class CardEntity
{
	public Guid Id { get; set; }
	public string CardCode { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public bool IsActive { get; set; }
	public Guid AccountId { get; set; }
	public virtual AccountEntity? ParentAccount { get; set; }
}
