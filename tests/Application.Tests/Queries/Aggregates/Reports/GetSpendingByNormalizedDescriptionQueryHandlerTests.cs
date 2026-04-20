using Application.Interfaces.Services;
using Application.Models.Reports;
using Application.Queries.Aggregates.Reports;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.Aggregates.Reports;

public class GetSpendingByNormalizedDescriptionQueryHandlerTests
{
	private readonly Mock<IReportService> _reportServiceMock;
	private readonly GetSpendingByNormalizedDescriptionQueryHandler _handler;

	public GetSpendingByNormalizedDescriptionQueryHandlerTests()
	{
		_reportServiceMock = new Mock<IReportService>();
		_handler = new GetSpendingByNormalizedDescriptionQueryHandler(_reportServiceMock.Object);
	}

	[Fact]
	public async Task Handle_DelegatesToReportService_WithNullDates()
	{
		// Arrange
		GetSpendingByNormalizedDescriptionQuery query = new(null, null);
		SpendingByNormalizedDescriptionResult expectedResult = new([], null, null);

		_reportServiceMock
			.Setup(s => s.GetSpendingByNormalizedDescriptionAsync(null, null, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		// Act
		SpendingByNormalizedDescriptionResult result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expectedResult);
		_reportServiceMock.Verify(
			s => s.GetSpendingByNormalizedDescriptionAsync(null, null, It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Handle_PassesDateRangeToService()
	{
		// Arrange
		DateTimeOffset from = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
		DateTimeOffset to = new(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
		GetSpendingByNormalizedDescriptionQuery query = new(from, to);
		SpendingByNormalizedDescriptionResult expectedResult = new([], from, to);

		_reportServiceMock
			.Setup(s => s.GetSpendingByNormalizedDescriptionAsync(from, to, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		// Act
		await _handler.Handle(query, CancellationToken.None);

		// Assert
		_reportServiceMock.Verify(
			s => s.GetSpendingByNormalizedDescriptionAsync(from, to, It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Handle_ReturnsServiceResult()
	{
		// Arrange
		GetSpendingByNormalizedDescriptionQuery query = new(null, null);
		SpendingByNormalizedDescriptionItem item = new(
			"Bananas",
			42.50m,
			"USD",
			5,
			new DateTimeOffset(2025, 1, 5, 0, 0, 0, TimeSpan.Zero),
			new DateTimeOffset(2025, 3, 15, 0, 0, 0, TimeSpan.Zero));
		SpendingByNormalizedDescriptionResult expectedResult = new([item], null, null);

		_reportServiceMock
			.Setup(s => s.GetSpendingByNormalizedDescriptionAsync(
				It.IsAny<DateTimeOffset?>(),
				It.IsAny<DateTimeOffset?>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		// Act
		SpendingByNormalizedDescriptionResult result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Items.Should().ContainSingle();
		result.Items[0].CanonicalName.Should().Be("Bananas");
		result.Items[0].TotalAmount.Should().Be(42.50m);
		result.Items[0].Currency.Should().Be("USD");
		result.Items[0].ItemCount.Should().Be(5);
	}
}
