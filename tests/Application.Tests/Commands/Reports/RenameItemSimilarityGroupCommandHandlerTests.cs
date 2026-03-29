using Application.Commands.Reports;
using Application.Interfaces.Services;
using FluentAssertions;
using Moq;

namespace Application.Tests.Commands.Reports;

public class RenameItemSimilarityGroupCommandHandlerTests
{
	private readonly Mock<IReportService> _reportServiceMock;
	private readonly RenameItemSimilarityGroupCommandHandler _handler;

	public RenameItemSimilarityGroupCommandHandlerTests()
	{
		_reportServiceMock = new Mock<IReportService>();
		_handler = new RenameItemSimilarityGroupCommandHandler(_reportServiceMock.Object);
	}

	[Fact]
	public async Task Handle_DelegatesToReportService()
	{
		// Arrange
		List<Guid> itemIds = [Guid.NewGuid(), Guid.NewGuid()];
		RenameItemSimilarityGroupCommand command = new(itemIds, "New Description");

		_reportServiceMock.Setup(s => s.RenameItemsAsync(
			itemIds, "New Description", It.IsAny<CancellationToken>()))
			.ReturnsAsync(2);

		// Act
		int result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().Be(2);
		_reportServiceMock.Verify(s => s.RenameItemsAsync(
			itemIds, "New Description", It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ReturnsUpdatedCount()
	{
		// Arrange
		List<Guid> itemIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
		RenameItemSimilarityGroupCommand command = new(itemIds, "Unified Name");

		_reportServiceMock.Setup(s => s.RenameItemsAsync(
			It.IsAny<List<Guid>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(3);

		// Act
		int result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().Be(3);
	}
}
