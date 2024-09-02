using Application.Commands.Account;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.Account;

public class CreateAccountCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsCreatedAccounts()
	{
		Mock<IAccountRepository> mockRepository = new();
		CreateAccountCommandHandler handler = new(mockRepository.Object);

		List<Domain.Core.Account> input = AccountGenerator.GenerateList(1);

		mockRepository.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.Account>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(input);

		CreateAccountCommand command = new(input);
		List<Domain.Core.Account> result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(input.Count, result.Count);

		mockRepository.Verify(r => r.CreateAsync(It.Is<List<Domain.Core.Account>>(accounts =>
			accounts.All(input => result.Any(output =>
				output.AccountCode == input.AccountCode &&
				output.Name == input.Name &&
				output.IsActive == input.IsActive))),
			It.IsAny<CancellationToken>()), Times.Once);

		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}