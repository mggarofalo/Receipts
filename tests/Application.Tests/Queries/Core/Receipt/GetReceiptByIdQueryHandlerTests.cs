using Application.Interfaces.Services;
using Application.Queries.Core.Receipt;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.Receipt;

public class GetReceiptByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnReceipt_WhenReceiptExists()
	{
		Domain.Core.Receipt expected = ReceiptGenerator.Generate();

		Mock<IReceiptService> mockService = new();
		mockService.Setup(r => r.GetByIdAsync(expected.Id, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetReceiptByIdQueryHandler handler = new(mockService.Object);
		GetReceiptByIdQuery query = new(expected.Id);
		Domain.Core.Receipt? result = await handler.Handle(query, CancellationToken.None);

		Assert.NotNull(result);
		result.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenReceiptDoesNotExist()
	{
		Guid missingId = Guid.NewGuid();
		Mock<IReceiptService> mockService = new();
		mockService.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>())).ReturnsAsync((Domain.Core.Receipt?)null);

		GetReceiptByIdQueryHandler handler = new(mockService.Object);
		GetReceiptByIdQuery query = new(missingId);
		Domain.Core.Receipt? result = await handler.Handle(query, CancellationToken.None);

		Assert.Null(result);
	}
}