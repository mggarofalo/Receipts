using Application.Interfaces.Repositories;
using SampleData.Domain.Core;
using Moq;
using Application.Queries.Core.Receipt;

namespace Application.Tests.Queries.Core.Receipt;

public class GetAllReceiptsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllAccounts()
	{
		List<Domain.Core.Receipt> receipts = ReceiptGenerator.GenerateList(2);

		Mock<IReceiptRepository> mockRepository = new();
		mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(receipts);

		GetAllReceiptsQueryHandler handler = new(mockRepository.Object);
		GetAllReceiptsQuery query = new();

		List<Domain.Core.Receipt> result = await handler.Handle(query, CancellationToken.None);

		Assert.Equal(receipts.Count, result.Count);
		Assert.True(receipts.All(input => result.Any(output =>
			output.Id == input.Id &&
			output.Location == input.Location &&
			output.Date == input.Date &&
			output.TaxAmount == input.TaxAmount &&
			output.Description == input.Description)));

		mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}