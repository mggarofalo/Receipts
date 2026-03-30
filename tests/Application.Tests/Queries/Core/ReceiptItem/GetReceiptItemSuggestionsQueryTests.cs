using Application.Queries.Core.ReceiptItem.GetReceiptItemSuggestions;

namespace Application.Tests.Queries.Core.ReceiptItem;

public class GetReceiptItemSuggestionsQueryTests
{
	[Fact]
	public void Query_CanBeCreated()
	{
		GetReceiptItemSuggestionsQuery query = new("MILK", "Walmart", 10);
		Assert.Equal("MILK", query.ItemCode);
		Assert.Equal("Walmart", query.Location);
		Assert.Equal(10, query.Limit);
	}

	[Fact]
	public void Query_WithNullLocation_CanBeCreated()
	{
		GetReceiptItemSuggestionsQuery query = new("MILK", null, 5);
		Assert.Equal("MILK", query.ItemCode);
		Assert.Null(query.Location);
		Assert.Equal(5, query.Limit);
	}

	[Fact]
	public void Query_WithDefaultLimit_UsesDefault()
	{
		GetReceiptItemSuggestionsQuery query = new("MILK", "Walmart");
		Assert.Equal(10, query.Limit);
	}
}
