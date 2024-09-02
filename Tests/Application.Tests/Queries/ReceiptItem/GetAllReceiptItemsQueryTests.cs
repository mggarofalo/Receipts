using Application.Queries.ReceiptItem;

namespace Application.Tests.Queries.ReceiptItem;

public class GetAllReceiptItemsQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetAllReceiptItemsQuery query = new();
		Assert.NotNull(query);
	}
}