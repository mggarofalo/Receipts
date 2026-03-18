using Application.Commands.Receipt.CreateComplete;
using Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Receipt.CreateComplete;

public class CreateCompleteReceiptCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_DelegatesToService()
	{
		// Arrange
		Mock<ICompleteReceiptService> mockService = new();
		CreateCompleteReceiptCommandHandler handler = new(mockService.Object);

		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		List<Domain.Core.Transaction> transactions = TransactionGenerator.GenerateList(2);
		List<Domain.Core.ReceiptItem> items = ReceiptItemGenerator.GenerateList(1);

		CreateCompleteReceiptResult expectedResult = new(receipt, transactions, items);

		mockService.Setup(s => s.CreateCompleteReceiptAsync(
			It.IsAny<Domain.Core.Receipt>(),
			It.IsAny<List<Domain.Core.Transaction>>(),
			It.IsAny<List<Domain.Core.ReceiptItem>>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		CreateCompleteReceiptCommand command = new(receipt, transactions, items);

		// Act
		CreateCompleteReceiptResult result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().Be(expectedResult);
		mockService.Verify(s => s.CreateCompleteReceiptAsync(
			It.IsAny<Domain.Core.Receipt>(),
			It.IsAny<List<Domain.Core.Transaction>>(),
			It.IsAny<List<Domain.Core.ReceiptItem>>(),
			It.IsAny<CancellationToken>()), Times.Once);
	}
}
