using Application.Queries.Aggregates.ReceiptsWithItems;
using Application.Queries.Aggregates.TransactionAccounts;
using Application.Queries.Aggregates.Trips;
using Domain.Aggregates;
using FluentAssertions;
using MediatR;
using Moq;
using SampleData.Domain.Aggregates;

namespace Application.Tests.Queries.Aggregates.Trips;

public class GetTripByReceiptIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnTrip_WhenReceiptWithItemsAndTransactionAccountsExist()
	{
		// Arrange
		Trip expected = TripGenerator.Generate();

		Mock<IMediator> mockMediator = new();
		mockMediator.Setup(m => m.Send(It.IsAny<GetReceiptWithItemsByReceiptIdQuery>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected.Receipt);
		mockMediator.Setup(m => m.Send(It.IsAny<GetTransactionAccountsByReceiptIdQuery>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected.Transactions);

		GetTripByReceiptIdQueryHandler handler = new(mockMediator.Object);
		GetTripByReceiptIdQuery query = new(expected.Receipt.Receipt.Id!.Value);

		// Act
		Trip? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		result.Receipt.Should().BeSameAs(expected.Receipt);
		result.Transactions.Should().BeSameAs(expected.Transactions);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenReceiptWithItemsDoesNotExist()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();

		Mock<IMediator> mockMediator = new();
		mockMediator.Setup(m => m.Send(It.IsAny<GetReceiptWithItemsByReceiptIdQuery>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((ReceiptWithItems?)null);

		GetTripByReceiptIdQueryHandler handler = new(mockMediator.Object);
		GetTripByReceiptIdQuery query = new(receiptId);

		// Act
		Trip? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task Handle_ShouldReturnNull_WhenTransactionAccountsDoNotExist()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		ReceiptWithItems receiptWithItems = ReceiptWithItemsGenerator.Generate();

		Mock<IMediator> mockMediator = new();
		mockMediator.Setup(m => m.Send(It.IsAny<GetReceiptWithItemsByReceiptIdQuery>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(receiptWithItems);
		mockMediator.Setup(m => m.Send(It.IsAny<GetTransactionAccountsByReceiptIdQuery>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync((List<TransactionAccount>?)null);

		GetTripByReceiptIdQueryHandler handler = new(mockMediator.Object);
		GetTripByReceiptIdQuery query = new(receiptId);

		// Act
		Trip? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.Null(result);
	}
}