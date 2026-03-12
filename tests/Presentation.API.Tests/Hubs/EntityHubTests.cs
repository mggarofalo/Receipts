using System.Security.Claims;
using API.Hubs;
using API.Services;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Presentation.API.Tests.Hubs;

public class EntityHubTests
{
	private readonly Mock<IHubCallerClients<IEntityHubClient>> _mockClients;
	private readonly Mock<IEntityHubClient> _mockClientProxy;
	private readonly Mock<ISignalRConnectionTracker> _mockTracker;
	private readonly Mock<HubCallerContext> _mockContext;
	private readonly EntityHub _hub;

	public EntityHubTests()
	{
		_mockClients = new Mock<IHubCallerClients<IEntityHubClient>>();
		_mockClientProxy = new Mock<IEntityHubClient>();
		_mockClients.Setup(clients => clients.All).Returns(_mockClientProxy.Object);
		_mockTracker = new Mock<ISignalRConnectionTracker>();

		_mockContext = new Mock<HubCallerContext>();
		_mockContext.Setup(c => c.ConnectionId).Returns("conn-123");

		_hub = new EntityHub(_mockTracker.Object)
		{
			Clients = _mockClients.Object,
			Context = _mockContext.Object,
		};
	}

	[Fact]
	public async Task OnConnectedAsync_TracksConnectionForAuthenticatedUser()
	{
		// Arrange
		var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "user-42") };
		var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
		_mockContext.Setup(c => c.User).Returns(principal);

		// Act
		await _hub.OnConnectedAsync();

		// Assert
		_mockTracker.Verify(t => t.TrackConnection("conn-123", "user-42"), Times.Once);
	}

	[Fact]
	public async Task OnConnectedAsync_DoesNotTrackWhenNoUserId()
	{
		// Arrange — no user claims
		_mockContext.Setup(c => c.User).Returns(new ClaimsPrincipal());

		// Act
		await _hub.OnConnectedAsync();

		// Assert
		_mockTracker.Verify(t => t.TrackConnection(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public async Task OnDisconnectedAsync_RemovesConnection()
	{
		// Act
		await _hub.OnDisconnectedAsync(null);

		// Assert
		_mockTracker.Verify(t => t.RemoveConnection("conn-123"), Times.Once);
	}

	[Fact]
	public async Task OnDisconnectedAsync_RemovesConnectionOnError()
	{
		// Act
		await _hub.OnDisconnectedAsync(new InvalidOperationException("test"));

		// Assert
		_mockTracker.Verify(t => t.RemoveConnection("conn-123"), Times.Once);
	}
}
