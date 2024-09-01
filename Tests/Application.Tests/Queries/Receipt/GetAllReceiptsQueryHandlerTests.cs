using Application.Queries.Receipt;
using Application.Interfaces.Repositories;
using Domain;
using Moq;

namespace Application.Tests.Queries.Account;

public class GetAllReceiptsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllAccounts()
	{
		List<Domain.Core.Receipt> allReceipts =
		[
			new(Guid.NewGuid(), "Location 1", new DateOnly(2021, 1, 1), new Money(100), "Description 1"),
			new(Guid.NewGuid(), "Location 2", new DateOnly(2021, 1, 2), new Money(200), "Description 2")
		];

		Mock<IReceiptRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(allReceipts);

		GetAllReceiptsQueryHandler handler = new(mockRepository.Object);
		GetAllReceiptsQuery query = new();

		List<Domain.Core.Receipt> result = await handler.Handle(query, CancellationToken.None);

		Assert.Equal(allReceipts.Count, result.Count);
		Assert.True(allReceipts.All(r => result.Any(rr =>
			rr.Id == r.Id &&
			rr.Location == r.Location &&
			rr.Date == r.Date &&
			rr.TaxAmount == r.TaxAmount &&
			rr.Description == r.Description)));

		mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}