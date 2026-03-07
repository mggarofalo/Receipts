using Application.Models;
using Application.Queries.Core.ReceiptItem;

namespace Application.Tests.Queries.Core.ReceiptItem;

public class GetAllReceiptItemsQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetAllReceiptItemsQuery query = new(0, 50, SortParams.Default);
		Assert.NotNull(query);
	}
}