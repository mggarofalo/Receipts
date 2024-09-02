using Application.Commands.Receipt;
using Application.Interfaces.Repositories;
using Domain;
using Moq;

namespace Application.Tests.Commands.Receipt;

public class CreateReceiptCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsCreatedAccounts()
	{
		Mock<IReceiptRepository> mockRepository = new();
		CreateReceiptCommandHandler handler = new(mockRepository.Object);

		List<Domain.Core.Receipt> inputReceipts =
		[
			new(null, "Location 1", new DateOnly(2021, 1, 1), new Money(100), "Description 1"),
			new(null, "Location 2", new DateOnly(2021, 1, 2), new Money(200), "Description 2")
		];

		CreateReceiptCommand command = new(inputReceipts);

		List<Domain.Core.Receipt> createdReceipts =
		[
			new(Guid.NewGuid(), "Location 1", new DateOnly(2021, 1, 1), new Money(100), "Description 1"),
			new(Guid.NewGuid(), "Location 2", new DateOnly(2021, 1, 2), new Money(200), "Description 2")
		];

		mockRepository.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.Receipt>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(createdReceipts);

		List<Domain.Core.Receipt> result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(createdReceipts.Count, result.Count);
		Assert.Equal(createdReceipts, result);

		mockRepository.Verify(r => r.CreateAsync(It.Is<List<Domain.Core.Receipt>>(receipts =>
			receipts.Count() == inputReceipts.Count &&
			receipts.All(r => inputReceipts.Any(ir =>
				ir.Location == r.Location &&
				ir.Date == r.Date &&
				ir.TaxAmount == r.TaxAmount &&
				ir.Description == r.Description))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}