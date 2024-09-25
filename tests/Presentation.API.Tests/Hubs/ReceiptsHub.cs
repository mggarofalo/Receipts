using API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Presentation.API.Tests.Hubs;

public class ReceiptsHubTests
{
	private readonly Mock<IHubCallerClients> _mockClients;
	private readonly Mock<IClientProxy> _mockClientProxy;
	private readonly ReceiptsHub _hub;

	public ReceiptsHubTests()
	{
		_mockClients = new Mock<IHubCallerClients>();
		_mockClientProxy = new Mock<IClientProxy>();
		_mockClients.Setup(clients => clients.All).Returns(_mockClientProxy.Object);

		_hub = new ReceiptsHub
		{
			Clients = _mockClients.Object,
			Context = new Mock<HubCallerContext>().Object
		};
	}

	[Fact]
	public async Task SendMessage_ShouldSendToAllClients()
	{
		// Arrange
		string user = "testUser";
		string message = "testMessage";

		// Act
		await _hub.SendMessage(user, message);

		// Assert
		_mockClientProxy.Verify(
			x => x.SendCoreAsync(
				"ReceiveMessage",
				It.Is<object[]>(o => o != null && o.Length == 2 && (string)o[0] == user && (string)o[1] == message),
				default),
			Times.Once);
	}

	[Fact]
	public async Task OnConnectedAsync_ShouldNotifyAllClients()
	{
		// Arrange
		Mock<HubCallerContext> mockContext = new();
		mockContext.Setup(m => m.ConnectionId).Returns("testConnectionId");
		_hub.Context = mockContext.Object;

		// Act
		await _hub.OnConnectedAsync();

		// Assert
		_mockClientProxy.Verify(
			x => x.SendCoreAsync(
				"UserConnected",
				It.Is<object[]>(o => o != null && o.Length == 1 && (string)o[0] == "testConnectionId"),
				default),
			Times.Once);
	}

	[Fact]
	public async Task OnDisconnectedAsync_ShouldNotifyAllClients()
	{
		// Arrange
		Mock<HubCallerContext> mockContext = new();
		mockContext.Setup(m => m.ConnectionId).Returns("testConnectionId");
		_hub.Context = mockContext.Object;

		// Act
		await _hub.OnDisconnectedAsync(null);

		// Assert
		_mockClientProxy.Verify(
			x => x.SendCoreAsync(
				"UserDisconnected",
				It.Is<object[]>(o => o != null && o.Length == 1 && (string)o[0] == "testConnectionId"),
				default),
			Times.Once);
	}
}
