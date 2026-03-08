using Application.Interfaces.Services;
using Application.Models.Dashboard;
using Application.Queries.Aggregates.Dashboard;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.Aggregates.Dashboard;

public class GetSpendingByCategoryQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnSpendingByCategory()
	{
		// Arrange
		DateOnly startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
		DateOnly endDate = DateOnly.FromDateTime(DateTime.Today);
		int limit = 10;

		SpendingByCategoryResult expected = new(
		[
			new SpendingCategoryItemResult("Groceries", 500.00m, 50.00m),
			new SpendingCategoryItemResult("Electronics", 300.00m, 30.00m),
			new SpendingCategoryItemResult("Dining", 200.00m, 20.00m)
		]);

		Mock<IDashboardService> mockService = new();
		mockService.Setup(s => s.GetSpendingByCategoryAsync(startDate, endDate, limit, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		GetSpendingByCategoryQueryHandler handler = new(mockService.Object);
		GetSpendingByCategoryQuery query = new(startDate, endDate, limit);

		// Act
		SpendingByCategoryResult result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expected);
		mockService.Verify(s => s.GetSpendingByCategoryAsync(startDate, endDate, limit, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldPassLimitToService()
	{
		// Arrange
		DateOnly startDate = new(2025, 1, 1);
		DateOnly endDate = new(2025, 12, 31);
		int limit = 5;

		SpendingByCategoryResult expected = new([]);

		Mock<IDashboardService> mockService = new();
		mockService.Setup(s => s.GetSpendingByCategoryAsync(startDate, endDate, limit, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		GetSpendingByCategoryQueryHandler handler = new(mockService.Object);
		GetSpendingByCategoryQuery query = new(startDate, endDate, limit);

		// Act
		await handler.Handle(query, CancellationToken.None);

		// Assert
		mockService.Verify(s => s.GetSpendingByCategoryAsync(startDate, endDate, limit, It.IsAny<CancellationToken>()), Times.Once);
	}
}
