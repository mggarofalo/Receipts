using Application.Commands.Transaction;
using Application.Interfaces.Repositories;
using Domain;
using Moq;

namespace Application.Tests.Commands.Transaction;

public class UpdateTransactionCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsUpdateAndSaveChanges()
	{
		Mock<ITransactionRepository> mockRepository = new();
		UpdateTransactionCommandHandler handler = new(mockRepository.Object);

		List<Domain.Core.Transaction> updatedTransactions =
		[
			new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Money(100), new DateOnly(2024, 1, 1)),
			new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Money(200), new DateOnly(2024, 1, 2))
		];

		UpdateTransactionCommand command = new(updatedTransactions);

		mockRepository.Setup(r => r
			.UpdateAsync(It.IsAny<List<Domain.Core.Transaction>>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);

		mockRepository.Verify(r => r.UpdateAsync(It.Is<List<Domain.Core.Transaction>>(transactions =>
			transactions.Count() == updatedTransactions.Count &&
			transactions.All(t => updatedTransactions.Any(ut =>
				ut.Id == t.Id &&
				ut.ReceiptId == t.ReceiptId &&
				ut.AccountId == t.AccountId &&
				ut.Amount == t.Amount &&
				ut.Date == t.Date))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}