using Application.Interfaces.Services;
using Application.Models.Dashboard;
using Application.Queries.Aggregates.Dashboard;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.Aggregates.Dashboard;

public class GetSpendingOverTimeQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnSpendingOverTime()
	{
		// Arrange
		DateOnly startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
		DateOnly endDate = DateOnly.FromDateTime(DateTime.Today);
		string granularity = "monthly";

		SpendingOverTimeResult expected = new(
		[
			new SpendingBucketResult("2025-01", 100.00m),
			new SpendingBucketResult("2025-02", 200.00m)
		]);

		Mock<IDashboardService> mockService = new();
		mockService.Setup(s => s.GetSpendingOverTimeAsync(startDate, endDate, granularity, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		GetSpendingOverTimeQueryHandler handler = new(mockService.Object);
		GetSpendingOverTimeQuery query = new(startDate, endDate, granularity);

		// Act
		SpendingOverTimeResult result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expected);
		mockService.Verify(s => s.GetSpendingOverTimeAsync(startDate, endDate, granularity, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Theory]
	[InlineData("daily")]
	[InlineData("weekly")]
	[InlineData("monthly")]
	public async Task Handle_ShouldPassGranularityToService(string granularity)
	{
		// Arrange
		DateOnly startDate = new(2025, 1, 1);
		DateOnly endDate = new(2025, 12, 31);

		SpendingOverTimeResult expected = new([]);

		Mock<IDashboardService> mockService = new();
		mockService.Setup(s => s.GetSpendingOverTimeAsync(startDate, endDate, granularity, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		GetSpendingOverTimeQueryHandler handler = new(mockService.Object);
		GetSpendingOverTimeQuery query = new(startDate, endDate, granularity);

		// Act
		await handler.Handle(query, CancellationToken.None);

		// Assert
		mockService.Verify(s => s.GetSpendingOverTimeAsync(startDate, endDate, granularity, It.IsAny<CancellationToken>()), Times.Once);
	}
}
