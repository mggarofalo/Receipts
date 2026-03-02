using Application.Queries.Core.Adjustment;

namespace Application.Tests.Queries.Core.Adjustment;

public class GetAdjustmentByIdQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		Guid id = Guid.NewGuid();
		GetAdjustmentByIdQuery query = new(id);
		Assert.Equal(id, query.Id);
	}

	[Fact]
	public void Query_WithEmptyId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new GetAdjustmentByIdQuery(Guid.Empty));
		Assert.StartsWith(GetAdjustmentByIdQuery.IdCannotBeEmptyExceptionMessage, exception.Message);
		Assert.Equal("id", exception.ParamName);
	}
}
