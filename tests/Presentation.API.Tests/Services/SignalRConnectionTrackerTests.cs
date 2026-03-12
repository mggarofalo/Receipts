using API.Services;
using FluentAssertions;

namespace Presentation.API.Tests.Services;

public class SignalRConnectionTrackerTests
{
	private readonly SignalRConnectionTracker _tracker = new();

	[Fact]
	public void TrackConnection_ThenIsConnectionOwnedBy_ReturnsTrueForCorrectUser()
	{
		// Arrange
		_tracker.TrackConnection("conn-1", "user-A");

		// Act
		bool result = _tracker.IsConnectionOwnedBy("conn-1", "user-A");

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void IsConnectionOwnedBy_ReturnsFalseForWrongUser()
	{
		// Arrange
		_tracker.TrackConnection("conn-1", "user-A");

		// Act
		bool result = _tracker.IsConnectionOwnedBy("conn-1", "user-B");

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsConnectionOwnedBy_ReturnsFalseForUnknownConnection()
	{
		// Act
		bool result = _tracker.IsConnectionOwnedBy("unknown-conn", "user-A");

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void RemoveConnection_MakesIsConnectionOwnedByReturnFalse()
	{
		// Arrange
		_tracker.TrackConnection("conn-1", "user-A");

		// Act
		_tracker.RemoveConnection("conn-1");

		// Assert
		_tracker.IsConnectionOwnedBy("conn-1", "user-A").Should().BeFalse();
	}

	[Fact]
	public void MultipleConnections_ForSameUser_AllValidateCorrectly()
	{
		// Arrange
		_tracker.TrackConnection("conn-1", "user-A");
		_tracker.TrackConnection("conn-2", "user-A");
		_tracker.TrackConnection("conn-3", "user-A");

		// Act & Assert
		_tracker.IsConnectionOwnedBy("conn-1", "user-A").Should().BeTrue();
		_tracker.IsConnectionOwnedBy("conn-2", "user-A").Should().BeTrue();
		_tracker.IsConnectionOwnedBy("conn-3", "user-A").Should().BeTrue();
	}

	[Fact]
	public void TrackConnection_WithExistingConnectionId_OverwritesPreviousUser()
	{
		// Arrange
		_tracker.TrackConnection("conn-1", "user-A");

		// Act
		_tracker.TrackConnection("conn-1", "user-B");

		// Assert
		_tracker.IsConnectionOwnedBy("conn-1", "user-B").Should().BeTrue();
		_tracker.IsConnectionOwnedBy("conn-1", "user-A").Should().BeFalse();
	}
}
