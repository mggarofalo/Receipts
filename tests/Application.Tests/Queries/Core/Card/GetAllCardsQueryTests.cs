using Application.Models;
using Application.Queries.Core.Card;

namespace Application.Tests.Queries.Core.Card;

public class GetAllCardsQueryTests : IQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetAllCardsQuery query = new(0, 50, SortParams.Default);
		Assert.NotNull(query);
	}
}