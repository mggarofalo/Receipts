using API.Hubs;
using API.Services;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Presentation.API.Tests.Services;

public class EntityChangeNotifierTests : IDisposable
{
	private readonly Mock<IHubContext<EntityHub, IEntityHubClient>> _hubContextMock;
	private readonly Mock<IEntityHubClient> _clientMock;
	private readonly EntityChangeNotifier _notifier;

	public EntityChangeNotifierTests()
	{
		_clientMock = new Mock<IEntityHubClient>();
		_clientMock.Setup(c => c.EntityChanged(It.IsAny<EntityChangeNotification>()))
			.Returns(Task.CompletedTask);

		Mock<IHubClients<IEntityHubClient>> hubClientsMock = new();
		hubClientsMock.Setup(c => c.All).Returns(_clientMock.Object);

		_hubContextMock = new Mock<IHubContext<EntityHub, IEntityHubClient>>();
		_hubContextMock.Setup(h => h.Clients).Returns(hubClientsMock.Object);

		// Use a very long flush interval so the timer never fires during tests
		_notifier = new EntityChangeNotifier(_hubContextMock.Object, TimeSpan.FromHours(1));
	}

	public void Dispose()
	{
		_notifier.Dispose();
		GC.SuppressFinalize(this);
	}

	[Fact]
	public async Task NotifyCreated_EnqueuesAndFlushSendsNotificationWithCount1()
	{
		// Arrange
		Guid id = Guid.NewGuid();

		// Act
		await _notifier.NotifyCreated("receipt", id);
		await _notifier.FlushAsync();

		// Assert
		_clientMock.Verify(c => c.EntityChanged(
			It.Is<EntityChangeNotification>(n =>
				n.EntityType == "receipt" &&
				n.ChangeType == "created" &&
				n.Count == 1)),
			Times.Once);
	}

	[Fact]
	public async Task NotifyUpdated_EnqueuesAndFlushSendsNotificationWithCount1()
	{
		// Arrange
		Guid id = Guid.NewGuid();

		// Act
		await _notifier.NotifyUpdated("account", id);
		await _notifier.FlushAsync();

		// Assert
		_clientMock.Verify(c => c.EntityChanged(
			It.Is<EntityChangeNotification>(n =>
				n.EntityType == "account" &&
				n.ChangeType == "updated" &&
				n.Count == 1)),
			Times.Once);
	}

	[Fact]
	public async Task NotifyDeleted_EnqueuesAndFlushSendsNotificationWithCount1()
	{
		// Arrange
		Guid id = Guid.NewGuid();

		// Act
		await _notifier.NotifyDeleted("category", id);
		await _notifier.FlushAsync();

		// Assert
		_clientMock.Verify(c => c.EntityChanged(
			It.Is<EntityChangeNotification>(n =>
				n.EntityType == "category" &&
				n.ChangeType == "deleted" &&
				n.Count == 1)),
			Times.Once);
	}

	[Fact]
	public async Task NotifyBulkChanged_With3Ids_SendsOneNotificationWithCount3()
	{
		// Arrange
		List<Guid> ids = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

		// Act
		await _notifier.NotifyBulkChanged("transaction", "created", ids);
		await _notifier.FlushAsync();

		// Assert
		_clientMock.Verify(c => c.EntityChanged(
			It.Is<EntityChangeNotification>(n =>
				n.EntityType == "transaction" &&
				n.ChangeType == "created" &&
				n.Count == 3)),
			Times.Once);
		_clientMock.Verify(c => c.EntityChanged(It.IsAny<EntityChangeNotification>()), Times.Once);
	}

	[Fact]
	public async Task NotifyBulkChanged_WithEmptyIds_SendsNothingOnFlush()
	{
		// Act
		await _notifier.NotifyBulkChanged("receipt", "deleted", []);
		await _notifier.FlushAsync();

		// Assert
		_clientMock.Verify(c => c.EntityChanged(It.IsAny<EntityChangeNotification>()), Times.Never);
	}

	[Fact]
	public async Task NotifyAllChanged_SendsNotificationWithCount1AndNullId()
	{
		// Act
		await _notifier.NotifyAllChanged("category", "updated");
		await _notifier.FlushAsync();

		// Assert
		_clientMock.Verify(c => c.EntityChanged(
			It.Is<EntityChangeNotification>(n =>
				n.EntityType == "category" &&
				n.ChangeType == "updated" &&
				n.Id == null &&
				n.Count == 1)),
			Times.Once);
	}

	[Fact]
	public async Task MultipleDifferentPairs_ProduceSeparateNotificationsOnFlush()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();

		// Act
		await _notifier.NotifyCreated("receipt", id1);
		await _notifier.NotifyDeleted("category", id2);
		await _notifier.FlushAsync();

		// Assert
		_clientMock.Verify(c => c.EntityChanged(
			It.Is<EntityChangeNotification>(n =>
				n.EntityType == "receipt" &&
				n.ChangeType == "created" &&
				n.Count == 1)),
			Times.Once);
		_clientMock.Verify(c => c.EntityChanged(
			It.Is<EntityChangeNotification>(n =>
				n.EntityType == "category" &&
				n.ChangeType == "deleted" &&
				n.Count == 1)),
			Times.Once);
		_clientMock.Verify(c => c.EntityChanged(It.IsAny<EntityChangeNotification>()), Times.Exactly(2));
	}

	[Fact]
	public async Task Dispose_FlushesPendingNotifications()
	{
		// Arrange — use a separate notifier for this test so Dispose actually flushes
		var clientMock = new Mock<IEntityHubClient>();
		clientMock.Setup(c => c.EntityChanged(It.IsAny<EntityChangeNotification>()))
			.Returns(Task.CompletedTask);
		Mock<IHubClients<IEntityHubClient>> hubClientsMock = new();
		hubClientsMock.Setup(c => c.All).Returns(clientMock.Object);
		Mock<IHubContext<EntityHub, IEntityHubClient>> hubContextMock = new();
		hubContextMock.Setup(h => h.Clients).Returns(hubClientsMock.Object);
		var notifier = new EntityChangeNotifier(hubContextMock.Object, TimeSpan.FromHours(1));

		Guid id = Guid.NewGuid();
		await notifier.NotifyCreated("receipt", id);

		// Act
		notifier.Dispose();

		// Assert
		clientMock.Verify(c => c.EntityChanged(
			It.Is<EntityChangeNotification>(n =>
				n.EntityType == "receipt" &&
				n.ChangeType == "created" &&
				n.Count == 1)),
			Times.Once);
	}

	[Fact]
	public async Task FlushAsync_WithNoPendingItems_SendsNothing()
	{
		// Act
		await _notifier.FlushAsync();

		// Assert
		_clientMock.Verify(c => c.EntityChanged(It.IsAny<EntityChangeNotification>()), Times.Never);
	}
}
