using Application.Commands.Account;
using Application.Interfaces.Repositories;
using Moq;

namespace Application.Tests.Commands.Account;

public class UpdateAccountCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsUpdateAndSaveChanges()
	{
		Mock<IAccountRepository> mockRepository = new();
		UpdateAccountCommandHandler handler = new(mockRepository.Object);

		List<Domain.Core.Account> updatedAccounts =
		[
			new(Guid.NewGuid(), "ACCT_1", "Test Account 1", false),
			new(Guid.NewGuid(), "ACCT_2", "Test Account 2", false)
		];

		UpdateAccountCommand command = new(updatedAccounts);

		mockRepository.Setup(r => r
			.UpdateAsync(It.IsAny<List<Domain.Core.Account>>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);

		mockRepository.Verify(r => r.UpdateAsync(It.Is<List<Domain.Core.Account>>(accounts =>
			accounts.Count() == updatedAccounts.Count &&
			accounts.All(a => updatedAccounts.Any(ua =>
				ua.Id == a.Id &&
				ua.AccountCode == a.AccountCode &&
				ua.Name == a.Name &&
				ua.IsActive == a.IsActive))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}