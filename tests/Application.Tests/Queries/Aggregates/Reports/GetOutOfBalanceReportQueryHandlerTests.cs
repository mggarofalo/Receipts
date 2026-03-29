using Application.Interfaces.Services;
using Application.Models.Reports;
using Application.Queries.Aggregates.Reports;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.Aggregates.Reports;

public class GetOutOfBalanceReportQueryHandlerTests
{
	private readonly Mock<IReportService> _reportServiceMock;
	private readonly GetOutOfBalanceReportQueryHandler _handler;

	public GetOutOfBalanceReportQueryHandlerTests()
	{
		_reportServiceMock = new Mock<IReportService>();
		_handler = new GetOutOfBalanceReportQueryHandler(_reportServiceMock.Object);
	}

	[Fact]
	public async Task Handle_DelegatesToReportService()
	{
		// Arrange
		GetOutOfBalanceReportQuery query = new("date", "asc", 1, 50);
		OutOfBalanceResult expectedResult = new([], 0, 0m);

		_reportServiceMock.Setup(s => s.GetOutOfBalanceAsync(
			"date", "asc", 1, 50, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		// Act
		OutOfBalanceResult result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expectedResult);
		_reportServiceMock.Verify(s => s.GetOutOfBalanceAsync(
			"date", "asc", 1, 50, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_PassesAllParametersToService()
	{
		// Arrange
		GetOutOfBalanceReportQuery query = new("difference", "desc", 3, 25);
		OutOfBalanceResult expectedResult = new([], 0, 0m);

		_reportServiceMock.Setup(s => s.GetOutOfBalanceAsync(
			"difference", "desc", 3, 25, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		// Act
		await _handler.Handle(query, CancellationToken.None);

		// Assert
		_reportServiceMock.Verify(s => s.GetOutOfBalanceAsync(
			"difference", "desc", 3, 25, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ReturnsServiceResult()
	{
		// Arrange
		GetOutOfBalanceReportQuery query = new("date", "asc", 1, 50);
		OutOfBalanceResult expectedResult = new(
		[
			new OutOfBalanceItem(Guid.NewGuid(), "Store", new DateOnly(2025, 1, 1),
				10m, 1m, 0m, 11m, 15m, -4m),
		], 1, 4m);

		_reportServiceMock.Setup(s => s.GetOutOfBalanceAsync(
			It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
			It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		// Act
		OutOfBalanceResult result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.TotalCount.Should().Be(1);
		result.TotalDiscrepancy.Should().Be(4m);
		result.Items.Should().ContainSingle();
	}
}
