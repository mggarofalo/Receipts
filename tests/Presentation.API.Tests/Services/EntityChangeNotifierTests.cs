using API.Hubs;
using API.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Presentation.API.Tests.Services;

public class EntityChangeNotifierTests
{
	private readonly Mock<IHubContext<EntityHub, IEntityHubClient>> _hubContextMock;
	private readonly Mock<IEntityHubClient> _clientMock;
	private readonly EntityChangeNotifier _notifier;

	public EntityChangeNotifierTests()
	{
		_clientMock = new Mock<IEntityHubClient>();
		Mock<IHubClients<IEntityHubClient>> hubClientsMock = new();
		hubClientsMock.Setup(c => c.All).Returns(_clientMock.Object);
		_hubContextMock = new Mock<IHubContext<EntityHub, IEntityHubClient>>();
		_hubContextMock.Setup(h => h.Clients).Returns(hubClientsMock.Object);

		_notifier = new EntityChangeNotifier(_hubContextMock.Object);
	}

	[Fact]
	public async Task NotifyCreated_SendsEntityChangedWithCreatedChangeType()
	{
		// Arrange
		Guid id = Guid.NewGuid();

		// Act
		await _notifier.NotifyCreated("receipt", id);

		// Assert
		_clientMock.Verify(c => c.EntityChanged(
			It.Is<EntityChangeNotification>(n => n.EntityType == "receipt" && n.ChangeType == "created" && n.Id == id)),
			Times.Once);
	}

	[Fact]
	public async Task NotifyUpdated_SendsEntityChangedWithUpdatedChangeType()
	{
		// Arrange
		Guid id = Guid.NewGuid();

		// Act
		await _notifier.NotifyUpdated("account", id);

		// Assert
		_clientMock.Verify(c => c.EntityChanged(
			It.Is<EntityChangeNotification>(n => n.EntityType == "account" && n.ChangeType == "updated" && n.Id == id)),
			Times.Once);
	}

	[Fact]
	public async Task NotifyDeleted_SendsEntityChangedWithDeletedChangeType()
	{
		// Arrange
		Guid id = Guid.NewGuid();

		// Act
		await _notifier.NotifyDeleted("category", id);

		// Assert
		_clientMock.Verify(c => c.EntityChanged(
			It.Is<EntityChangeNotification>(n => n.EntityType == "category" && n.ChangeType == "deleted" && n.Id == id)),
			Times.Once);
	}

	[Fact]
	public async Task NotifyBulkChanged_SendsEntityChangedForEachId()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		Guid id3 = Guid.NewGuid();
		List<Guid> ids = [id1, id2, id3];

		// Act
		await _notifier.NotifyBulkChanged("transaction", "created", ids);

		// Assert
		_clientMock.Verify(c => c.EntityChanged(
			It.Is<EntityChangeNotification>(n => n.EntityType == "transaction" && n.ChangeType == "created" && n.Id == id1)),
			Times.Once);
		_clientMock.Verify(c => c.EntityChanged(
			It.Is<EntityChangeNotification>(n => n.EntityType == "transaction" && n.ChangeType == "created" && n.Id == id2)),
			Times.Once);
		_clientMock.Verify(c => c.EntityChanged(
			It.Is<EntityChangeNotification>(n => n.EntityType == "transaction" && n.ChangeType == "created" && n.Id == id3)),
			Times.Once);
		_clientMock.Verify(c => c.EntityChanged(It.IsAny<EntityChangeNotification>()), Times.Exactly(3));
	}

	[Fact]
	public async Task NotifyBulkChanged_WithEmptyIds_DoesNotSendAnyNotifications()
	{
		// Act
		await _notifier.NotifyBulkChanged("receipt", "deleted", []);

		// Assert
		_clientMock.Verify(c => c.EntityChanged(It.IsAny<EntityChangeNotification>()), Times.Never);
	}
}
