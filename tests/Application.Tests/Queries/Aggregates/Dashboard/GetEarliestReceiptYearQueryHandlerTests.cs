using Application.Interfaces.Services;
using Application.Queries.Aggregates.Dashboard;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.Aggregates.Dashboard;

public class GetEarliestReceiptYearQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnEarliestReceiptYear()
	{
		// Arrange
		Mock<IDashboardService> mockService = new();
		mockService.Setup(s => s.GetEarliestReceiptYearAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(2020);

		GetEarliestReceiptYearQueryHandler handler = new(mockService.Object);
		GetEarliestReceiptYearQuery query = new();

		// Act
		int result = await handler.Handle(query, CancellationToken.None);

		// Assert
		result.Should().Be(2020);
		mockService.Verify(s => s.GetEarliestReceiptYearAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}
