using FluentAssertions;
using SampleData.ViewModels.Aggregates;
using SampleData.ViewModels.Core;
using Shared.ViewModels.Aggregates;

namespace Presentation.Shared.Tests.ViewModels.Aggregates;

public class TripVMTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesTripVM()
	{
		// Arrange
		ReceiptWithItemsVM expectedReceipt = ReceiptWithItemsVMGenerator.Generate();
		List<TransactionAccountVM> expectedTransactions =
		[
			TransactionAccountVMGenerator.Generate(),
			TransactionAccountVMGenerator.Generate()
		];

		// Act
		TripVM tripVM = new()
		{
			Receipt = expectedReceipt,
			Transactions = expectedTransactions
		};

		// Assert
		tripVM.Receipt.Should().BeSameAs(expectedReceipt);
		tripVM.Transactions.Should().BeSameAs(expectedTransactions);
		Assert.Equal(2, tripVM.Transactions.Count);
	}
}
