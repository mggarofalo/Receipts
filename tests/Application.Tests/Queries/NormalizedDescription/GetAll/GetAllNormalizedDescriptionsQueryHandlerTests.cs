using Application.Interfaces.Services;
using Application.Queries.NormalizedDescription.GetAll;
using Domain.NormalizedDescriptions;
using FluentAssertions;
using Moq;

namespace Application.Tests.Queries.NormalizedDescription.GetAll;

public class GetAllNormalizedDescriptionsQueryHandlerTests
{
	[Fact]
	public async Task Handle_NoFilter_ReturnsAllFromService()
	{
		// Arrange
		Mock<INormalizedDescriptionService> mockService = new();
		List<Domain.NormalizedDescriptions.NormalizedDescription> expected =
		[
			new(Guid.NewGuid(), "coffee beans", NormalizedDescriptionStatus.Active, DateTimeOffset.UtcNow),
			new(Guid.NewGuid(), "whole milk", NormalizedDescriptionStatus.PendingReview, DateTimeOffset.UtcNow),
		];

		mockService
			.Setup(s => s.GetAllAsync(null, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		GetAllNormalizedDescriptionsQueryHandler handler = new(mockService.Object);
		GetAllNormalizedDescriptionsQuery query = new(StatusFilter: null);

		// Act
		List<Domain.NormalizedDescriptions.NormalizedDescription> actual = await handler.Handle(query, CancellationToken.None);

		// Assert
		actual.Should().BeSameAs(expected);
		mockService.Verify(s => s.GetAllAsync(null, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WithFilter_ForwardsToService()
	{
		Mock<INormalizedDescriptionService> mockService = new();
		List<Domain.NormalizedDescriptions.NormalizedDescription> expected =
		[
			new(Guid.NewGuid(), "whole milk", NormalizedDescriptionStatus.PendingReview, DateTimeOffset.UtcNow),
		];

		mockService
			.Setup(s => s.GetAllAsync(NormalizedDescriptionStatus.PendingReview, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		GetAllNormalizedDescriptionsQueryHandler handler = new(mockService.Object);
		GetAllNormalizedDescriptionsQuery query = new(NormalizedDescriptionStatus.PendingReview);

		List<Domain.NormalizedDescriptions.NormalizedDescription> actual = await handler.Handle(query, CancellationToken.None);

		actual.Should().BeSameAs(expected);
		mockService.Verify(s => s.GetAllAsync(NormalizedDescriptionStatus.PendingReview, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_EmptyResult_ReturnsEmptyList()
	{
		Mock<INormalizedDescriptionService> mockService = new();
		mockService
			.Setup(s => s.GetAllAsync(It.IsAny<NormalizedDescriptionStatus?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);

		GetAllNormalizedDescriptionsQueryHandler handler = new(mockService.Object);
		GetAllNormalizedDescriptionsQuery query = new(NormalizedDescriptionStatus.Active);

		List<Domain.NormalizedDescriptions.NormalizedDescription> actual = await handler.Handle(query, CancellationToken.None);

		actual.Should().BeEmpty();
	}
}
