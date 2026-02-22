using API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Presentation.API.Tests.Hubs;

public class ReceiptsHubTests
{
	private readonly Mock<IHubCallerClients<IReceiptsHubClient>> _mockClients;
	private readonly Mock<IReceiptsHubClient> _mockClientProxy;
	private readonly Mock<IGroupManager> _mockGroups;
	private readonly ReceiptsHub _hub;

	public ReceiptsHubTests()
	{
		_mockClients = new Mock<IHubCallerClients<IReceiptsHubClient>>();
		_mockClientProxy = new Mock<IReceiptsHubClient>();
		_mockGroups = new Mock<IGroupManager>();
		_mockClients.Setup(clients => clients.All).Returns(_mockClientProxy.Object);

		_hub = new ReceiptsHub
		{
			Clients = _mockClients.Object,
			Context = new Mock<HubCallerContext>().Object,
			Groups = _mockGroups.Object
		};
	}

	private static Mock<HubCallerContext> CreateAuthenticatedContext(string userId, string connectionId = "test-connection-id")
	{
		Mock<HubCallerContext> context = new();
		context.Setup(c => c.UserIdentifier).Returns(userId);
		context.Setup(c => c.ConnectionId).Returns(connectionId);
		return context;
	}

	[Fact]
	public async Task OnConnectedAsync_AddsConnectionToUserGroup_WhenUserIsAuthenticated()
	{
		// Arrange
		const string userId = "test-user-id";
		const string connectionId = "test-connection-id";
		_hub.Context = CreateAuthenticatedContext(userId, connectionId).Object;

		// Act
		await _hub.OnConnectedAsync();

		// Assert
		_mockGroups.Verify(
			g => g.AddToGroupAsync(connectionId, userId, It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task OnConnectedAsync_DoesNotAddToGroup_WhenUserIsNotAuthenticated()
	{
		// Arrange – Context.UserIdentifier is null by default on an unmocked HubCallerContext

		// Act
		await _hub.OnConnectedAsync();

		// Assert
		_mockGroups.Verify(
			g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task OnConnectedAsync_CompletesSuccessfully()
	{
		// Act & Assert – should not throw regardless of auth state
		await _hub.OnConnectedAsync();
	}
}
