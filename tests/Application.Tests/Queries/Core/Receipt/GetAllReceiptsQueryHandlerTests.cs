using SampleData.Domain.Core;
using Moq;
using Application.Queries.Core.Receipt;
using Application.Interfaces.Services;

namespace Application.Tests.Queries.Core.Receipt;

public class GetAllReceiptsQueryHandlerTests
{
	[Fact]
	public async Task Handle_ShouldReturnAllAccounts()
	{
		List<Domain.Core.Receipt> expected = ReceiptGenerator.GenerateList(2);

		Mock<IReceiptService> mockService = new();
		mockService.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

		GetAllReceiptsQueryHandler handler = new(mockService.Object);
		GetAllReceiptsQuery query = new();

		List<Domain.Core.Receipt> result = await handler.Handle(query, CancellationToken.None);

		Assert.Equal(expected.Count, result.Count);
		Assert.True(expected.All(result.Contains));
		Assert.True(result.All(expected.Contains));
	}
}