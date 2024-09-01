using Application.Commands.ReceiptItem;
using Application.Interfaces.Repositories;
using Domain;
using Moq;

namespace Application.Tests.Commands.ReceiptItem;

public class UpdateReceiptItemCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsUpdateAndSaveChanges()
	{
		Mock<IReceiptItemRepository> mockRepository = new();
		UpdateReceiptItemCommandHandler handler = new(mockRepository.Object);

		List<Domain.Core.ReceiptItem> updatedReceiptItems =
		[
			new(Guid.NewGuid(), Guid.NewGuid(), "Item 1", "Description 1", 1, new Money(10), new Money(10), "Tax 1", "Tax 2"),
			new(Guid.NewGuid(), Guid.NewGuid(), "Item 2", "Description 2", 2, new Money(20), new Money(40), "Tax 3", "Tax 4")
		];

		UpdateReceiptItemCommand command = new(updatedReceiptItems);

		mockRepository.Setup(r => r
			.UpdateAsync(It.IsAny<List<Domain.Core.ReceiptItem>>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);

		mockRepository.Verify(r => r.UpdateAsync(It.Is<List<Domain.Core.ReceiptItem>>(receiptItems =>
			receiptItems.Count() == updatedReceiptItems.Count &&
			receiptItems.All(ri => updatedReceiptItems.Any(uri =>
				uri.Id == ri.Id &&
				uri.ReceiptId == ri.ReceiptId &&
				uri.ReceiptItemCode == ri.ReceiptItemCode &&
				uri.Description == ri.Description &&
				uri.Quantity == ri.Quantity &&
				uri.UnitPrice == ri.UnitPrice &&
				uri.TotalAmount == ri.TotalAmount &&
				uri.Category == ri.Category &&
				uri.Subcategory == ri.Subcategory))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}