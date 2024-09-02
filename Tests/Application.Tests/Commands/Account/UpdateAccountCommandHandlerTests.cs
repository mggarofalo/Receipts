using Application.Commands.Account;
using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;

namespace Application.Tests.Commands.Account;

public class UpdateAccountCommandHandlerTests
{
	[Fact]
	public async Task Handle_WithValidCommand_ReturnsTrueAndCallsUpdateAndSaveChanges()
	{
		Mock<IAccountRepository> mockRepository = new();
		UpdateAccountCommandHandler handler = new(mockRepository.Object);

		List<Domain.Core.Account> input = AccountGenerator.GenerateList(2);

		mockRepository.Setup(r => r
			.UpdateAsync(It.IsAny<List<Domain.Core.Account>>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		UpdateAccountCommand command = new(input);
		bool result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result);

		mockRepository.Verify(r => r.UpdateAsync(It.IsAny<List<Domain.Core.Account>>(), It.IsAny<CancellationToken>()), Times.Once);
		mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}