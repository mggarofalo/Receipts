using Application.Interfaces.Services;
using Application.Models;
using Application.Queries.Core.Transaction;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Queries.Core.Transaction;

public class GetTransactionsByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnTransactions_WhenReceiptExistsAndHasItems()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		List<Domain.Core.Transaction> expected = TransactionGenerator.GenerateList(2);

		Mock<ITransactionService> mockService = new();
		mockService.Setup(r => r.GetByReceiptIdAsync(receipt.Id, 0, 50, It.IsAny<SortParams>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PagedResult<Domain.Core.Transaction>(expected, expected.Count, 0, 50));

		GetTransactionsByReceiptIdQueryHandler handler = new(mockService.Object);
		GetTransactionsByReceiptIdQuery query = new(receipt.Id, 0, 50, SortParams.Default);

		PagedResult<Domain.Core.Transaction> result = await handler.Handle(query, CancellationToken.None);

		result.Data.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task Handle_ShouldReturnEmptyList_WhenReceiptHasNoItems()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();

		Mock<ITransactionService> mockService = new();
		mockService.Setup(r => r.GetByReceiptIdAsync(receipt.Id, 0, 50, It.IsAny<SortParams>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PagedResult<Domain.Core.Transaction>([], 0, 0, 50));

		GetTransactionsByReceiptIdQueryHandler handler = new(mockService.Object);
		GetTransactionsByReceiptIdQuery query = new(receipt.Id, 0, 50, SortParams.Default);

		PagedResult<Domain.Core.Transaction> result = await handler.Handle(query, CancellationToken.None);

		result.Data.Should().BeEmpty();
		result.Total.Should().Be(0);
	}

	[Fact]
	public async Task Handle_ShouldReturnEmpty_WhenReceiptDoesNotExist()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();

		Mock<ITransactionService> mockService = new();
		mockService.Setup(r => r.GetByReceiptIdAsync(receipt.Id, 0, 50, It.IsAny<SortParams>(), It.IsAny<CancellationToken>())).ReturnsAsync(new PagedResult<Domain.Core.Transaction>([], 0, 0, 50));

		GetTransactionsByReceiptIdQueryHandler handler = new(mockService.Object);
		GetTransactionsByReceiptIdQuery query = new(receipt.Id, 0, 50, SortParams.Default);

		PagedResult<Domain.Core.Transaction> result = await handler.Handle(query, CancellationToken.None);

		result.Data.Should().BeEmpty();
		result.Total.Should().Be(0);
	}
}
