using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;
using Application.Queries.Core.ReceiptItem;

namespace Application.Tests.Queries.Core.ReceiptItem;

public class GetReceiptItemByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnReceiptItem_WhenReceiptItemExists()
	{
		Domain.Core.ReceiptItem expected = ReceiptItemGenerator.Generate();

		Mock<IReceiptItemRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetReceiptItemByIdQueryHandler handler = new(mockRepository.Object);
		GetReceiptItemByIdQuery query = new(expected.Id!.Value);
		Domain.Core.ReceiptItem? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(expected, result);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenAccountDoesNotExist()
	{
		Mock<IReceiptItemRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.ReceiptItem?)null);

		GetReceiptItemByIdQueryHandler handler = new(mockRepository.Object);
		GetReceiptItemByIdQuery query = new(Guid.NewGuid());
		Domain.Core.ReceiptItem? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
	}
}