using Application.Commands.Receipt;
using Application.Interfaces.Repositories;
using Domain;
using Moq;

namespace Application.Tests.Commands.Receipt;

public class UpdateReceiptCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsUpdateAndSaveChanges()
	{
		Mock<IReceiptRepository> mockRepository = new();
		UpdateReceiptCommandHandler handler = new(mockRepository.Object);

		List<Domain.Core.Receipt> updatedReceipts =
		[
			new(Guid.NewGuid(), "Location 1", new DateOnly(2024, 1, 1), new Money(100)),
			new(Guid.NewGuid(), "Location 2", new DateOnly(2024, 1, 2), new Money(200))
		];

		UpdateReceiptCommand command = new(updatedReceipts);

		mockRepository.Setup(r => r
			.UpdateAsync(It.IsAny<List<Domain.Core.Receipt>>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);

		mockRepository.Verify(r => r.UpdateAsync(It.Is<List<Domain.Core.Receipt>>(receipts =>
			receipts.Count() == updatedReceipts.Count &&
			receipts.All(r => updatedReceipts.Any(ur =>
				ur.Id == r.Id &&
				ur.Location == r.Location &&
				ur.Date == r.Date &&
				ur.TaxAmount == r.TaxAmount))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}