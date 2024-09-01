using Application.Queries.ReceiptItem;
using Application.Interfaces.Repositories;
using Domain;
using Moq;

namespace Application.Tests.Queries.ReceiptItem;

public class GetAllReceiptItemsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllReceiptItems()
	{
		List<Domain.Core.ReceiptItem> allReceiptItems =
		[
			new(Guid.NewGuid(), Guid.NewGuid(), "Test Receipt Item 1", "Test Receipt Item 1", 1, new Money(10), new Money(10), "Test Category 1", "Test Subcategory 1"),
			new(Guid.NewGuid(), Guid.NewGuid(), "Test Receipt Item 2", "Test Receipt Item 2", 2, new Money(7), new Money(14), "Test Category 2", "Test Subcategory 2")
		];

		Mock<IReceiptItemRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(allReceiptItems);

		GetAllReceiptItemsQueryHandler handler = new(mockRepository.Object);
		GetAllReceiptItemsQuery query = new();

		List<Domain.Core.ReceiptItem> result = await handler.Handle(query, CancellationToken.None);

		Assert.Equal(allReceiptItems.Count, result.Count);
		Assert.True(allReceiptItems.All(ri => result.Any(rr =>
			rr.ReceiptId == ri.ReceiptId &&
			rr.ReceiptItemCode == ri.ReceiptItemCode &&
			rr.Description == ri.Description &&
			rr.Quantity == ri.Quantity &&
			rr.UnitPrice == ri.UnitPrice &&
			rr.TotalAmount == ri.TotalAmount &&
			rr.Category == ri.Category &&
			rr.Subcategory == ri.Subcategory)));

		mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}