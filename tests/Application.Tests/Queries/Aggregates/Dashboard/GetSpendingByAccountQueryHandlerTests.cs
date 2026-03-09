using Application.Interfaces.Services;
using Application.Models.Dashboard;
using Application.Queries.Aggregates.Dashboard;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.Aggregates.Dashboard;

public class GetSpendingByAccountQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnSpendingByAccount()
	{
		// Arrange
		DateOnly startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
		DateOnly endDate = DateOnly.FromDateTime(DateTime.Today);

		Guid visaId = Guid.NewGuid();
		Guid amexId = Guid.NewGuid();

		SpendingByAccountResult expected = new(
		[
			new SpendingAccountItemResult(visaId, "Visa", 600.00m, 60.00m),
			new SpendingAccountItemResult(amexId, "Amex", 400.00m, 40.00m)
		]);

		Mock<IDashboardService> mockService = new();
		mockService.Setup(s => s.GetSpendingByAccountAsync(startDate, endDate, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		GetSpendingByAccountQueryHandler handler = new(mockService.Object);
		GetSpendingByAccountQuery query = new(startDate, endDate);

		// Act
		SpendingByAccountResult result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expected);
		mockService.Verify(s => s.GetSpendingByAccountAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ShouldPassDatesToService()
	{
		// Arrange
		DateOnly startDate = new(2025, 3, 1);
		DateOnly endDate = new(2025, 3, 31);

		SpendingByAccountResult expected = new([]);

		Mock<IDashboardService> mockService = new();
		mockService.Setup(s => s.GetSpendingByAccountAsync(startDate, endDate, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		GetSpendingByAccountQueryHandler handler = new(mockService.Object);
		GetSpendingByAccountQuery query = new(startDate, endDate);

		// Act
		await handler.Handle(query, CancellationToken.None);

		// Assert
		mockService.Verify(s => s.GetSpendingByAccountAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
	}
}
