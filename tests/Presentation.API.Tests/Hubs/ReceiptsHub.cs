using API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Presentation.API.Tests.Hubs;

public class ReceiptsHubTests
{
	private readonly Mock<IHubCallerClients<IReceiptsHubClient>> _mockClients;
	private readonly Mock<IReceiptsHubClient> _mockClientProxy;
	private readonly ReceiptsHub _hub;

	public ReceiptsHubTests()
	{
		_mockClients = new Mock<IHubCallerClients<IReceiptsHubClient>>();
		_mockClientProxy = new Mock<IReceiptsHubClient>();
		_mockClients.Setup(clients => clients.All).Returns(_mockClientProxy.Object);

		_hub = new ReceiptsHub
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
