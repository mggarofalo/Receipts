using Application.Interfaces.Services;
using Application.Models.Reports;
using Application.Queries.Aggregates.Reports;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.Aggregates.Reports;

public class GetSpendingByLocationReportQueryHandlerTests
{
	private readonly Mock<IReportService> _reportServiceMock;
	private readonly GetSpendingByLocationReportQueryHandler _handler;

	public GetSpendingByLocationReportQueryHandlerTests()
	{
		_reportServiceMock = new Mock<IReportService>();
		_handler = new GetSpendingByLocationReportQueryHandler(_reportServiceMock.Object);
	}

	[Fact]
	public async Task Handle_DelegatesToReportService()
	{
		// Arrange
		GetSpendingByLocationReportQuery query = new(null, null, "total", "desc", 1, 50);
		SpendingByLocationResult expectedResult = new([], 0, 0m);

		_reportServiceMock.Setup(s => s.GetSpendingByLocationAsync(
			null, null, "total", "desc", 1, 50, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		// Act
		SpendingByLocationResult result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expectedResult);
		_reportServiceMock.Verify(s => s.GetSpendingByLocationAsync(
			null, null, "total", "desc", 1, 50, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_PassesAllParametersToService()
	{
		// Arrange
		DateOnly start = new(2025, 1, 1);
		DateOnly end = new(2025, 12, 31);
		GetSpendingByLocationReportQuery query = new(start, end, "visits", "asc", 3, 25);
		SpendingByLocationResult expectedResult = new([], 0, 0m);

		_reportServiceMock.Setup(s => s.GetSpendingByLocationAsync(
			start, end, "visits", "asc", 3, 25, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		// Act
		await _handler.Handle(query, CancellationToken.None);

		// Assert
		_reportServiceMock.Verify(s => s.GetSpendingByLocationAsync(
			start, end, "visits", "asc", 3, 25, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ReturnsServiceResult()
	{
		// Arrange
		GetSpendingByLocationReportQuery query = new(null, null, "total", "desc", 1, 50);
		SpendingByLocationResult expectedResult = new(
		[
			new SpendingByLocationItem("Store A", 5, 100.50m, 20.10m),
		], 1, 100.50m);

		_reportServiceMock.Setup(s => s.GetSpendingByLocationAsync(
			It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(),
			It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		// Act
		SpendingByLocationResult result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.TotalCount.Should().Be(1);
		result.GrandTotal.Should().Be(100.50m);
		result.Items.Should().ContainSingle();
		result.Items[0].Location.Should().Be("Store A");
		result.Items[0].Visits.Should().Be(5);
		result.Items[0].Total.Should().Be(100.50m);
		result.Items[0].AveragePerVisit.Should().Be(20.10m);
	}
}
