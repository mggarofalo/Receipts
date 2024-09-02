using Application.Commands.ReceiptItem;
using Application.Interfaces.Repositories;
using Domain;
using Moq;

namespace Application.Tests.Commands.ReceiptItem;

public class CreateReceiptItemCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsCreatedAccounts()
	{
		Mock<IReceiptItemRepository> mockRepository = new();
		CreateReceiptItemCommandHandler handler = new(mockRepository.Object);

		Guid receiptId = Guid.NewGuid();

		List<Domain.Core.ReceiptItem> inputReceiptItems =
		[
			new(null, receiptId, "Test Receipt Item 1", "Test Receipt Item 1", 1, new Money(10), new Money(10), "Test Category 1", "Test Subcategory 1"),
			new(null, receiptId, "Test Receipt Item 2", "Test Receipt Item 2", 2, new Money(7), new Money(14), "Test Category 2", "Test Subcategory 2")
		];

		CreateReceiptItemCommand command = new(inputReceiptItems);

		List<Domain.Core.ReceiptItem> createdReceiptItems =
		[
			new(Guid.NewGuid(), receiptId, "Test Receipt Item 1", "Test Receipt Item 1", 1, new Money(10), new Money(10), "Test Category 1", "Test Subcategory 1"),
			new(Guid.NewGuid(), receiptId, "Test Receipt Item 2", "Test Receipt Item 2", 2, new Money(7), new Money(14), "Test Category 2", "Test Subcategory 2")
		];

		mockRepository.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.ReceiptItem>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(createdReceiptItems);

		List<Domain.Core.ReceiptItem> result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(createdReceiptItems.Count, result.Count);
		Assert.Equal(createdReceiptItems, result);

		mockRepository.Verify(r => r.CreateAsync(It.Is<List<Domain.Core.ReceiptItem>>(receiptItems =>
			receiptItems.Count() == inputReceiptItems.Count &&
			receiptItems.All(ri => inputReceiptItems.Any(iri =>
				iri.ReceiptId == ri.ReceiptId &&
				iri.ReceiptItemCode == ri.ReceiptItemCode &&
				iri.Description == ri.Description &&
				iri.Quantity == ri.Quantity &&
				iri.UnitPrice == ri.UnitPrice &&
				iri.TotalAmount == ri.TotalAmount &&
				iri.Category == ri.Category &&
				iri.Subcategory == ri.Subcategory))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}