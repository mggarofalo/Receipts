using Application.Models;
using Application.Queries.Core.Adjustment;

namespace Application.Tests.Queries.Core.Adjustment;

public class GetAdjustmentsByReceiptIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid receiptId = Guid.NewGuid();
		GetAdjustmentsByReceiptIdQuery query = new(receiptId, 0, 50, SortParams.Default);
		Assert.Equal(receiptId, query.ReceiptId);
	}

	[Fact]
	public void Query_WithEmptyReceiptId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetAdjustmentsByReceiptIdQuery(Guid.Empty, 0, 50, SortParams.Default));
		Assert.StartsWith(GetAdjustmentsByReceiptIdQuery.ReceiptIdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("receiptId", exception.ParamName);
	}
}
