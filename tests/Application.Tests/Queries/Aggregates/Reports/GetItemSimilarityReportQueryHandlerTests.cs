using Application.Interfaces.Services;
using Application.Models.Reports;
using Application.Queries.Aggregates.Reports;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.Aggregates.Reports;

public class GetItemSimilarityReportQueryHandlerTests
{
	private readonly Mock<IReportService> _reportServiceMock;
	private readonly GetItemSimilarityReportQueryHandler _handler;

	public GetItemSimilarityReportQueryHandlerTests()
	{
		_reportServiceMock = new Mock<IReportService>();
		_handler = new GetItemSimilarityReportQueryHandler(_reportServiceMock.Object);
	}

	[Fact]
	public async Task Handle_DelegatesToReportService()
	{
		// Arrange
		GetItemSimilarityReportQuery query = new(0.7, "occurrences", "desc", 1, 50);
		ItemSimilarityResult expectedResult = new([], 0);

		_reportServiceMock.Setup(s => s.GetItemSimilarityAsync(
			0.7, "occurrences", "desc", 1, 50, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		// Act
		ItemSimilarityResult result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expectedResult);
		_reportServiceMock.Verify(s => s.GetItemSimilarityAsync(
			0.7, "occurrences", "desc", 1, 50, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_PassesAllParametersToService()
	{
		// Arrange
		GetItemSimilarityReportQuery query = new(0.5, "canonicalName", "asc", 3, 25);
		ItemSimilarityResult expectedResult = new([], 0);

		_reportServiceMock.Setup(s => s.GetItemSimilarityAsync(
			0.5, "canonicalName", "asc", 3, 25, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		// Act
		await _handler.Handle(query, CancellationToken.None);

		// Assert
		_reportServiceMock.Verify(s => s.GetItemSimilarityAsync(
			0.5, "canonicalName", "asc", 3, 25, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_ReturnsServiceResult()
	{
		// Arrange
		GetItemSimilarityReportQuery query = new(0.7, "occurrences", "desc", 1, 50);
		ItemSimilarityResult expectedResult = new(
		[
			new ItemSimilarityGroup(
				"Milk",
				["Milk", "MILK", "milk 2%"],
				[Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()],
				3,
				0.85),
		], 1);

		_reportServiceMock.Setup(s => s.GetItemSimilarityAsync(
			It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>(),
			It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		// Act
		ItemSimilarityResult result = await _handler.Handle(query, CancellationToken.None);

		// Assert
		result.TotalCount.Should().Be(1);
		result.Groups.Should().ContainSingle();
		result.Groups[0].CanonicalName.Should().Be("Milk");
		result.Groups[0].Occurrences.Should().Be(3);
	}
}
