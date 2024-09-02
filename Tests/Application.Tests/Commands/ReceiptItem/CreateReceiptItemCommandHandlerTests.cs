using Application.Commands.ReceiptItem;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.ReceiptItem;

public class CreateReceiptItemCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsCreatedReceiptItems()
	{
		Mock<IReceiptItemRepository> mockRepository = new();
		CreateReceiptItemCommandHandler handler = new(mockRepository.Object);

		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		List<Domain.Core.ReceiptItem> input = ReceiptItemGenerator.GenerateList(2, receipt.Id!.Value);

		mockRepository.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.ReceiptItem>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(input);

		CreateReceiptItemCommand command = new(input);
		List<Domain.Core.ReceiptItem> result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(input.Count, result.Count);

		mockRepository.Verify(r => r.CreateAsync(It.Is<List<Domain.Core.ReceiptItem>>(receiptItems =>
			receiptItems.All(input => result.Any(output =>
				output.ReceiptId == input.ReceiptId &&
				output.ReceiptItemCode == input.ReceiptItemCode &&
				output.Description == input.Description &&
				output.Quantity == input.Quantity &&
				output.UnitPrice == input.UnitPrice &&
				output.TotalAmount == input.TotalAmount &&
				output.Category == input.Category &&
				output.Subcategory == input.Subcategory))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}