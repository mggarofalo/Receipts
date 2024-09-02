using Application.Queries.Aggregates.ReceiptsWithItems;
using Application.Queries.Aggregates.TransactionAccounts;
using Application.Queries.Aggregates.Trips;
using Domain.Aggregates;
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
		Trip trip = TripGenerator.Generate();

		Mock<IMediator> mockMediator = new();
		mockMediator.Setup(m => m.Send(It.IsAny<GetReceiptWithItemsByReceiptIdQuery>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(trip.Receipt);
		mockMediator.Setup(m => m.Send(It.IsAny<GetTransactionAccountsByReceiptIdQuery>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(trip.Transactions);

		GetTripByReceiptIdQueryHandler handler = new(mockMediator.Object);
		GetTripByReceiptIdQuery query = new(trip.Receipt.Receipt.Id!.Value);

		// Act
		Trip? result = await handler.Handle(query, CancellationToken.None);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(trip.Receipt, result.Receipt);
		Assert.Equal(trip.Transactions, result.Transactions);

		mockMediator.Verify(m => m.Send(It.Is<GetReceiptWithItemsByReceiptIdQuery>(q => q.ReceiptId == trip.Receipt.Receipt.Id), It.IsAny<CancellationToken>()), Times.Once);
		mockMediator.Verify(m => m.Send(It.Is<GetTransactionAccountsByReceiptIdQuery>(q => q.ReceiptId == trip.Receipt.Receipt.Id), It.IsAny<CancellationToken>()), Times.Once);
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
		mockMediator.Verify(m => m.Send(It.Is<GetReceiptWithItemsByReceiptIdQuery>(q => q.ReceiptId == receiptId), It.IsAny<CancellationToken>()), Times.Once);
		mockMediator.Verify(m => m.Send(It.IsAny<GetTransactionAccountsByReceiptIdQuery>(), It.IsAny<CancellationToken>()), Times.Never);
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
		mockMediator.Verify(m => m.Send(It.Is<GetReceiptWithItemsByReceiptIdQuery>(q => q.ReceiptId == receiptId), It.IsAny<CancellationToken>()), Times.Once);
		mockMediator.Verify(m => m.Send(It.Is<GetTransactionAccountsByReceiptIdQuery>(q => q.ReceiptId == receiptId), It.IsAny<CancellationToken>()), Times.Once);
	}
}