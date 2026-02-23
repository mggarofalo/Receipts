using API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Presentation.API.Tests.Hubs;

public class ReceiptsHubTests
{
	[Fact]
	public async Task OnConnectedAsync_AddsConnectionToUserGroup_WhenUserIdentifierExists()
	{
		// Arrange
		Mock<IHubCallerClients<IReceiptsHubClient>> mockClients = new();
		Mock<HubCallerContext> mockContext = new();
		Mock<IGroupManager> mockGroups = new();

		mockContext.Setup(c => c.UserIdentifier).Returns("user-123");
		mockContext.Setup(c => c.ConnectionId).Returns("conn-abc");

		ReceiptsHub hub = new()
		{
			Clients = mockClients.Object,
			Context = mockContext.Object,
			Groups = mockGroups.Object,
		};

		// Act
		await hub.OnConnectedAsync();

		// Assert
		mockGroups.Verify(
			g => g.AddToGroupAsync("conn-abc", "user-123", default),
			Times.Once);
	}

	[Fact]
	public async Task OnConnectedAsync_DoesNotAddGroup_WhenUserIdentifierIsNull()
	{
		// Arrange
		Mock<IHubCallerClients<IReceiptsHubClient>> mockClients = new();
		Mock<HubCallerContext> mockContext = new();
		Mock<IGroupManager> mockGroups = new();

		mockContext.Setup(c => c.UserIdentifier).Returns((string?)null);
		mockContext.Setup(c => c.ConnectionId).Returns("conn-abc");

		ReceiptsHub hub = new()
		{
			Clients = mockClients.Object,
			Context = mockContext.Object,
			Groups = mockGroups.Object,
		};

		// Act
		await hub.OnConnectedAsync();

		// Assert
		mockGroups.Verify(
			g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default),
			Times.Never);
	}

	[Fact]
	public async Task OnDisconnectedAsync_RemovesConnectionFromUserGroup_WhenUserIdentifierExists()
	{
		// Arrange
		Mock<IHubCallerClients<IReceiptsHubClient>> mockClients = new();
		Mock<HubCallerContext> mockContext = new();
		Mock<IGroupManager> mockGroups = new();

		mockContext.Setup(c => c.UserIdentifier).Returns("user-123");
		mockContext.Setup(c => c.ConnectionId).Returns("conn-abc");

		ReceiptsHub hub = new()
		{
			Clients = mockClients.Object,
			Context = mockContext.Object,
			Groups = mockGroups.Object,
		};

		// Act
		await hub.OnDisconnectedAsync(null);

		// Assert
		mockGroups.Verify(
			g => g.RemoveFromGroupAsync("conn-abc", "user-123", default),
			Times.Once);
	}
}
