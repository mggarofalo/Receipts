using Application.Interfaces.Services;
using Application.Models.Dashboard;
using Application.Queries.Aggregates.Dashboard;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.Aggregates.Dashboard;

public class GetSpendingByStoreQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnStoreSpending()
	{
		// Arrange
		DateOnly startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
		DateOnly endDate = DateOnly.FromDateTime(DateTime.Today);

		SpendingByStoreResult expected = new(
		[
			new SpendingByStoreItemResult("Walmart", 5, 250m, 50m),
			new SpendingByStoreItemResult("Target", 3, 150m, 50m),
		]);

		Mock<IDashboardService> mockService = new();
		mockService.Setup(s => s.GetSpendingByStoreAsync(startDate, endDate, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		GetSpendingByStoreQueryHandler handler = new(mockService.Object);
		GetSpendingByStoreQuery query = new(startDate, endDate);

		// Act
		SpendingByStoreResult result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expected);
		mockService.Verify(s => s.GetSpendingByStoreAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldPassDatesToService()
	{
		// Arrange
		DateOnly startDate = new(2025, 1, 1);
		DateOnly endDate = new(2025, 6, 30);

		SpendingByStoreResult expected = new([]);

		Mock<IDashboardService> mockService = new();
		mockService.Setup(s => s.GetSpendingByStoreAsync(startDate, endDate, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		GetSpendingByStoreQueryHandler handler = new(mockService.Object);
		GetSpendingByStoreQuery query = new(startDate, endDate);

		// Act
		await handler.Handle(query, CancellationToken.None);

		// Assert
		mockService.Verify(s => s.GetSpendingByStoreAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
	}
}
