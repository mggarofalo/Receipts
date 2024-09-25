using Application.Queries.Aggregates.Trips;

namespace Application.Tests.Queries.Aggregates.Trips;

public class GetTripByReceiptIdQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid id = Guid.NewGuid();
		GetTripByReceiptIdQuery query = new(id);
		Assert.Equal(id, query.ReceiptId);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetTripByReceiptIdQuery(Guid.Empty));
		Assert.StartsWith(GetTripByReceiptIdQuery.ReceiptIdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("receiptId", exception.ParamName);
	}
}