using Application.Interfaces;

namespace Application.Queries.ReceiptItem;

public record GetReceiptItemByIdQuery : IQuery<Domain.Core.ReceiptItem?>
{
	public Guid Id { get; }
	public const string IdCannotBeEmptyExceptionMessage = "ReceiptItem Id cannot be empty.";

	public GetReceiptItemByIdQuery(Guid id)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(IdCannotBeEmptyExceptionMessage, nameof(id));
		}

		Id = id;
	}
}
