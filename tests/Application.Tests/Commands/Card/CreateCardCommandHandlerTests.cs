using Application.Commands.Card.Create;
using Application.Interfaces.Services;
using FluentAssertions;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Card;

public class CreateCardCommandHandlerTests
{
	[Fact]
	public async Task CreateCardCommandHandler_WithValidCommand_ReturnsCreatedAccounts()
	{
		Mock<ICardService> mockService = new();
		CreateCardCommandHandler handler = new(mockService.Object);

		List<Domain.Core.Card> input = CardGenerator.GenerateList(1);

		mockService.Setup(r => r
			.CreateAsync(It.IsAny<List<Domain.Core.Card>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(input);

		CreateCardCommand command = new(input);
		List<Domain.Core.Card> result = await handler.Handle(command, CancellationToken.None);

		result.Should().HaveCount(input.Count);
	}
}