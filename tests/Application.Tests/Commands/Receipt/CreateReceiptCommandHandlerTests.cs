using Application.Commands.Receipt.Create;
using Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Receipt;

public class CreateReceiptCommandHandlerTests
{
	[Fact]
	public async Task CreateReceiptCommandHandler_WithValidCommand_ReturnsCreatedReceipts()
	{
		Mock<IReceiptService> mockService = new();
		CreateReceiptCommandHandler handler = new(mockService.Object);

		List<Domain.Core.Receipt> input = ReceiptGenerator.GenerateList(1);

		mockService.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.Receipt>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(input);

		CreateReceiptCommand command = new(input);
		List<Domain.Core.Receipt> result = await handler.Handle(command, CancellationToken.None);

		result.Should().HaveCount(input.Count);
	}
}