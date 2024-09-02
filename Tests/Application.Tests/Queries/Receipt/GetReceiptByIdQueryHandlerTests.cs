using Application.Interfaces.Repositories;
using Application.Queries.Receipt;
using Domain;
using Moq;

namespace Application.Tests.Queries.Receipt;

public class GetReceiptByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnReceipt_WhenReceiptExists()
	{
		Guid receiptId = Guid.NewGuid();
		Domain.Core.Receipt receipt = new(receiptId, "Location 1", new DateOnly(2021, 1, 1), new Money(100), "Description 1");

		Mock<IReceiptRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(receipt);

		GetReceiptByIdQueryHandler handler = new(mockRepository.Object);
		GetReceiptByIdQuery query = new(receiptId);

		Domain.Core.Receipt? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenReceiptDoesNotExist()
	{
		Mock<IReceiptRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Receipt?)null);

		GetReceiptByIdQueryHandler handler = new(mockRepository.Object);
		GetReceiptByIdQuery query = new(Guid.NewGuid());

		Domain.Core.Receipt? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
		mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
	}
}