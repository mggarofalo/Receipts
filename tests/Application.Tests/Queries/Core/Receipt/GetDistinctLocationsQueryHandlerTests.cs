using Application.Interfaces.Services;
using Application.Queries.Core.Receipt;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.Core.Receipt;

public class GetDistinctLocationsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnLocations_WhenQueryIsNull()
	{
		// Arrange
		List<string> expected = ["Walmart", "Target", "Costco"];

		Mock<IReceiptService> mockService = new();
		mockService.Setup(s => s.GetDistinctLocationsAsync(null, 20, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		GetDistinctLocationsQueryHandler handler = new(mockService.Object);
		GetDistinctLocationsQuery query = new(null, 20);

		// Act
		List<string> result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expected);
	}

	[Fact]
	public async Task Handle_ShouldPassQueryAndLimitToService()
	{
		// Arrange
		List<string> expected = ["Walmart"];

		Mock<IReceiptService> mockService = new();
		mockService.Setup(s => s.GetDistinctLocationsAsync("Wal", 5, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		GetDistinctLocationsQueryHandler handler = new(mockService.Object);
		GetDistinctLocationsQuery query = new("Wal", 5);

		// Act
		List<string> result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expected);
		mockService.Verify(s => s.GetDistinctLocationsAsync("Wal", 5, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldReturnEmptyList_WhenNoLocationsMatch()
	{
		// Arrange
		Mock<IReceiptService> mockService = new();
		mockService.Setup(s => s.GetDistinctLocationsAsync("xyz", 20, It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);

		GetDistinctLocationsQueryHandler handler = new(mockService.Object);
		GetDistinctLocationsQuery query = new("xyz", 20);

		// Act
		List<string> result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeEmpty();
	}
}
