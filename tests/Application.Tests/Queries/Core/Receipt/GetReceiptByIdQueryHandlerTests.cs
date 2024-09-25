using Application.Interfaces.Repositories;
using Application.Queries.Core.Receipt;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.Receipt;

public class GetReceiptByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnReceipt_WhenReceiptExists()
	{
		Domain.Core.Receipt expected = ReceiptGenerator.Generate();

		Mock<IReceiptRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetReceiptByIdQueryHandler handler = new(mockRepository.Object);
		GetReceiptByIdQuery query = new(expected.Id!.Value);
		Domain.Core.Receipt? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(expected, result);
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
	}
}