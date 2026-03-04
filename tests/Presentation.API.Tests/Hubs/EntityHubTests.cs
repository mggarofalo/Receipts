using API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Presentation.API.Tests.Hubs;

public class EntityHubTests
{
	private readonly Mock<IHubCallerClients<IEntityHubClient>> _mockClients;
	private readonly Mock<IEntityHubClient> _mockClientProxy;
	private readonly EntityHub _hub;

	public EntityHubTests()
	{
		_mockClients = new Mock<IHubCallerClients<IEntityHubClient>>();
		_mockClientProxy = new Mock<IEntityHubClient>();
		_mockClients.Setup(clients => clients.All).Returns(_mockClientProxy.Object);

		_hub = new EntityHub
		{
			Clients = _mockClients.Object,
			Context = new Mock<HubCallerContext>().Object
		};
	}

	[Fact]
	public async Task OnConnectedAsync_CompletesSuccessfully()
	{
		// Act & Assert - should not throw
		await _hub.OnConnectedAsync();
	}

	[Fact]
	public async Task OnDisconnectedAsync_CompletesSuccessfully()
	{
		// Act & Assert - should not throw
		await _hub.OnDisconnectedAsync(null);
	}
}
