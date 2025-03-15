using Application.Commands.Account.Create;
using SampleData.Domain.Core;
using Moq;
using Application.Interfaces.Services;

namespace Application.Tests.Commands.Account;

public class CreateAccountCommandHandlerTests
{
	[Fact]
	public async Task CreateAccountCommandHandler_WithValidCommand_ReturnsCreatedAccounts()
	{
		Mock<IAccountService> mockService = new();
		CreateAccountCommandHandler handler = new(mockService.Object);

		List<Domain.Core.Account> input = AccountGenerator.GenerateList(1);

		mockService.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.Account>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(input);

		CreateAccountCommand command = new(input);
		List<Domain.Core.Account> result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(input.Count, result.Count);
	}
}