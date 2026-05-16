using Application.Commands.Account.Create;
using Application.Interfaces.Services;
using Application.Queries.Core.Account;
using Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using DomainAccount = Domain.Core.Account;
using MediatorPkg = Mediator;

namespace Application.Tests.Mediator;

public class MediatorRegistrationTests
{
	private static IServiceProvider BuildProvider(IAccountService accountService)
	{
		ServiceCollection services = new();
		IConfiguration configuration = new ConfigurationBuilder().Build();
		services.AddSingleton(configuration);
		services.AddSingleton(accountService);
		services.RegisterApplicationServices(configuration);
		return services.BuildServiceProvider();
	}

	[Fact]
	public void MediatorIMediator_IsResolvable()
	{
		// Arrange
		Mock<IAccountService> accountServiceMock = new();
		IServiceProvider provider = BuildProvider(accountServiceMock.Object);

		// Act
		MediatorPkg.IMediator mediator = provider.GetRequiredService<MediatorPkg.IMediator>();

		// Assert
		mediator.Should().NotBeNull();
	}

	[Fact]
	public async Task GetAccountByIdQuery_DispatchesToHandler()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		DomainAccount expected = new(id, "Bank", true);
		Mock<IAccountService> accountServiceMock = new();
		accountServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		IServiceProvider provider = BuildProvider(accountServiceMock.Object);
		MediatorPkg.IMediator mediator = provider.GetRequiredService<MediatorPkg.IMediator>();

		// Act
		DomainAccount? result = await mediator.Send(new GetAccountByIdQuery(id));

		// Assert
		result.Should().Be(expected);
		accountServiceMock.Verify(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task CreateAccountCommand_DispatchesToHandler()
	{
		// Arrange
		DomainAccount account = new(Guid.NewGuid(), "Bank2", true);
		Mock<IAccountService> accountServiceMock = new();
		accountServiceMock.Setup(s => s.CreateAsync(It.IsAny<List<DomainAccount>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync([account]);

		IServiceProvider provider = BuildProvider(accountServiceMock.Object);
		MediatorPkg.IMediator mediator = provider.GetRequiredService<MediatorPkg.IMediator>();

		// Act
		List<DomainAccount> result = await mediator.Send(new CreateAccountCommand([account]));

		// Assert
		result.Should().ContainSingle().Which.Should().Be(account);
		accountServiceMock.Verify(s => s.CreateAsync(It.IsAny<List<DomainAccount>>(), It.IsAny<CancellationToken>()), Times.Once);
	}
}
