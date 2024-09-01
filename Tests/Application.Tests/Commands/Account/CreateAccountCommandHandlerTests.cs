using Application.Commands.Account;
using Application.Interfaces.Repositories;
using Moq;

namespace Application.Tests.Commands.Account;

public class CreateAccountCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsCreatedAccounts()
	{
		Mock<IAccountRepository> mockRepository = new();
		CreateAccountCommandHandler handler = new(mockRepository.Object);

		List<Domain.Core.Account> inputAccounts =
		[
			new(null, "ACCT_1", "Test Account 1", true),
			new(null, "ACCT_2", "Test Account 2", true)
		];

		CreateAccountCommand command = new(inputAccounts);

		List<Domain.Core.Account> createdAccounts =
		[
			new(Guid.NewGuid(), "ACCT_1", "Test Account 1", true),
			new(Guid.NewGuid(), "ACCT_2", "Test Account 2", true)
		];

		mockRepository.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.Account>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(createdAccounts);

		List<Domain.Core.Account> result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(createdAccounts.Count, result.Count);
		Assert.Equal(createdAccounts, result);

		mockRepository.Verify(r => r.CreateAsync(It.Is<List<Domain.Core.Account>>(accounts =>
			accounts.Count() == inputAccounts.Count &&
			accounts.All(a => inputAccounts.Any(ia =>
				ia.AccountCode == a.AccountCode &&
				ia.Name == a.Name &&
				ia.IsActive == a.IsActive))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}