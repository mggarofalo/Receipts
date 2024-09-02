using Application.Interfaces.Repositories;
using Application.Queries.ReceiptItem;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Queries.ReceiptItem;

public class GetAllReceiptItemsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllReceiptItems()
	{
		List<Domain.Core.ReceiptItem> receiptItems = ReceiptItemGenerator.GenerateList(2);

		Mock<IReceiptItemRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(receiptItems);

		GetAllReceiptItemsQueryHandler handler = new(mockRepository.Object);
		GetAllReceiptItemsQuery query = new();

		List<Domain.Core.ReceiptItem> result = await handler.Handle(query, CancellationToken.None);

		Assert.Equal(receiptItems.Count, result.Count);
		Assert.True(receiptItems.All(input => result.Any(output =>
			output.ReceiptId == input.ReceiptId &&
			output.ReceiptItemCode == input.ReceiptItemCode &&
			output.Description == input.Description &&
			output.Quantity == input.Quantity &&
			output.UnitPrice == input.UnitPrice &&
			output.TotalAmount == input.TotalAmount &&
			output.Category == input.Category &&
			output.Subcategory == input.Subcategory)));

		mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}