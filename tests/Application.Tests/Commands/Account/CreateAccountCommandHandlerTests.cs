using Application.Commands.Account;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.Account;

public class CreateAccountCommandHandlerTests
{
	[Fact]
	public async Task CreateAccountCommandHandler_WithValidCommand_ReturnsCreatedAccounts()
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
	}
}