using Application.Commands.Receipt;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.Receipt;

public class CreateReceiptCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsCreatedReceipts()
	{
		Mock<IReceiptRepository> mockRepository = new();
		CreateReceiptCommandHandler handler = new(mockRepository.Object);

		List<Domain.Core.Receipt> input = ReceiptGenerator.GenerateList(1);

		mockRepository.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.Receipt>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(input);

		CreateReceiptCommand command = new(input);
		List<Domain.Core.Receipt> result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(input.Count, result.Count);

		mockRepository.Verify(r => r.CreateAsync(It.Is<List<Domain.Core.Receipt>>(receipts =>
			receipts.All(input => result.Any(output =>
				output.Location == input.Location &&
				output.Date == input.Date &&
				output.TaxAmount == input.TaxAmount &&
				output.Description == input.Description))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}