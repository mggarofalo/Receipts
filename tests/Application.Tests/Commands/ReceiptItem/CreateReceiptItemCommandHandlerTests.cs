using Application.Commands.ReceiptItem.Create;
using Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.ReceiptItem;

public class CreateReceiptItemCommandHandlerTests
{
	[Fact]
	public async Task CreateReceiptItemCommandHandler_WithValidCommand_ReturnsCreatedReceiptItems()
	{
		Mock<IReceiptItemService> mockService = new();
		CreateReceiptItemCommandHandler handler = new(mockService.Object);

		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		List<Domain.Core.ReceiptItem> input = ReceiptItemGenerator.GenerateList(2);

		mockService.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.ReceiptItem>>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(input);

		CreateReceiptItemCommand command = new(input, receipt.Id);
		List<Domain.Core.ReceiptItem> result = await handler.Handle(command, CancellationToken.None);

		result.Should().HaveCount(input.Count);
	}
}