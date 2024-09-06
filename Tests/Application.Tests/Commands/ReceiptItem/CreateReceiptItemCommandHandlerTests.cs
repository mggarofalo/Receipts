using Application.Commands.ReceiptItem;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.ReceiptItem;

public class CreateReceiptItemCommandHandlerTests
{
	[Fact]
	public async Task CreateReceiptItemCommandHandler_WithValidCommand_ReturnsCreatedReceiptItems()
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
	}
}