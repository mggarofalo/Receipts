using Application.Commands.NormalizedDescription.Split;
using Application.Interfaces.Services;
using Domain.NormalizedDescriptions;
using FluentAssertions;
using Moq;

namespace Application.Tests.Commands.NormalizedDescription.Split;

public class SplitNormalizedDescriptionCommandHandlerTests
{
	[Fact]
	public async Task Handle_ForwardsReceiptItemIdAndReturnsCreated()
	{
		// Arrange
		Mock<INormalizedDescriptionService> mockService = new();
		Guid receiptItemId = Guid.NewGuid();
		Domain.NormalizedDescriptions.NormalizedDescription expected = new(
			Guid.NewGuid(),
			"cherry cola",
			NormalizedDescriptionStatus.Active,
			new DateTimeOffset(2026, 4, 19, 12, 0, 0, TimeSpan.Zero));

		mockService
			.Setup(s => s.SplitAsync(receiptItemId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		SplitNormalizedDescriptionCommandHandler handler = new(mockService.Object);
		SplitNormalizedDescriptionCommand command = new(receiptItemId);

		// Act
		Domain.NormalizedDescriptions.NormalizedDescription actual = await handler.Handle(command, CancellationToken.None);

		// Assert
		actual.Should().BeSameAs(expected);
		mockService.Verify(s => s.SplitAsync(receiptItemId, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ServicePropagatesKeyNotFound()
	{
		// The service throws KeyNotFoundException when the receipt item is missing; the
		// handler should propagate rather than swallow so controller/test callers can map
		// it to a 404.
		Mock<INormalizedDescriptionService> mockService = new();
		Guid missing = Guid.NewGuid();

		mockService
			.Setup(s => s.SplitAsync(missing, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new KeyNotFoundException("Receipt item not found."));

		SplitNormalizedDescriptionCommandHandler handler = new(mockService.Object);
		SplitNormalizedDescriptionCommand command = new(missing);

		Func<Task> act = () => handler.Handle(command, CancellationToken.None);
		await act.Should().ThrowAsync<KeyNotFoundException>();
	}
}
