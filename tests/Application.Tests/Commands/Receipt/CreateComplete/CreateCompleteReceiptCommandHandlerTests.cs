using Application.Commands.Receipt.CreateComplete;
using Application.Interfaces.Services;
using Domain;
using Domain.Core;
using FluentAssertions;
using FluentValidation;
using Moq;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Receipt.CreateComplete;

public class CreateCompleteReceiptCommandHandlerTests
{
	private readonly Mock<ICompleteReceiptService> _mockService = new();
	private readonly CreateCompleteReceiptCommandHandler _handler;

	public CreateCompleteReceiptCommandHandlerTests()
	{
		_handler = new CreateCompleteReceiptCommandHandler(_mockService.Object);
	}

	[Fact]
	public async Task Handle_WithBalancedTransactions_DelegatesToService()
	{
		// Arrange: receipt with $10 tax, 2 items at $5 each = $20 expected total
		Domain.Core.Receipt receipt = new(Guid.NewGuid(), "Test", DateOnly.FromDateTime(DateTime.Now), new Money(10));
		List<Domain.Core.ReceiptItem> items = ReceiptItemGenerator.GenerateList(2); // 2 x $5 = $10 subtotal
		decimal expectedTotal = items.Sum(i => i.TotalAmount.Amount) + receipt.TaxAmount.Amount; // $10 + $10 = $20

		List<Domain.Core.Transaction> transactions =
		[
			new Domain.Core.Transaction(Guid.NewGuid(), new Money(expectedTotal), DateOnly.FromDateTime(DateTime.Now))
		];

		CreateCompleteReceiptResult expectedResult = new(receipt, transactions, items);
		_mockService.Setup(s => s.CreateAsync(
				It.IsAny<Domain.Core.Receipt>(),
				It.IsAny<List<Domain.Core.Transaction>>(),
				It.IsAny<List<Domain.Core.ReceiptItem>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		CreateCompleteReceiptCommand command = new(receipt, transactions, items);

		// Act
		CreateCompleteReceiptResult result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expectedResult);
		_mockService.Verify(s => s.CreateAsync(
			It.IsAny<Domain.Core.Receipt>(),
			It.IsAny<List<Domain.Core.Transaction>>(),
			It.IsAny<List<Domain.Core.ReceiptItem>>(),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WithUnbalancedTransactions_ThrowsValidationException()
	{
		// Arrange: items total $10, tax $10, expected $20, but transactions total $15
		Domain.Core.Receipt receipt = new(Guid.NewGuid(), "Test", DateOnly.FromDateTime(DateTime.Now), new Money(10));
		List<Domain.Core.ReceiptItem> items = ReceiptItemGenerator.GenerateList(2); // 2 x $5 = $10 subtotal
		List<Domain.Core.Transaction> transactions =
		[
			new Domain.Core.Transaction(Guid.NewGuid(), new Money(15), DateOnly.FromDateTime(DateTime.Now))
		];

		CreateCompleteReceiptCommand command = new(receipt, transactions, items);

		// Act & Assert
		await Assert.ThrowsAsync<ValidationException>(() =>
			_handler.Handle(command, CancellationToken.None));

		_mockService.Verify(s => s.CreateAsync(
			It.IsAny<Domain.Core.Receipt>(),
			It.IsAny<List<Domain.Core.Transaction>>(),
			It.IsAny<List<Domain.Core.ReceiptItem>>(),
			It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WithNoTransactionsOrItems_DelegatesToService()
	{
		// Arrange: receipt only, no transactions or items — should not validate balance
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		CreateCompleteReceiptResult expectedResult = new(receipt, [], []);

		_mockService.Setup(s => s.CreateAsync(
				It.IsAny<Domain.Core.Receipt>(),
				It.IsAny<List<Domain.Core.Transaction>>(),
				It.IsAny<List<Domain.Core.ReceiptItem>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		CreateCompleteReceiptCommand command = new(receipt, [], []);

		// Act
		CreateCompleteReceiptResult result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expectedResult);
		_mockService.Verify(s => s.CreateAsync(
			It.IsAny<Domain.Core.Receipt>(),
			It.IsAny<List<Domain.Core.Transaction>>(),
			It.IsAny<List<Domain.Core.ReceiptItem>>(),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WithTransactionsButNoItems_Unbalanced_ThrowsValidationException()
	{
		// Arrange: no items means ExpectedTotal = TaxAmount ($10), but transaction is $100
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate(); // TaxAmount = $10
		List<Domain.Core.Transaction> transactions = TransactionGenerator.GenerateList(1); // Amount = $100

		CreateCompleteReceiptCommand command = new(receipt, transactions, []);

		// Act & Assert — $100 != $10, should throw
		await Assert.ThrowsAsync<ValidationException>(() =>
			_handler.Handle(command, CancellationToken.None));

		_mockService.Verify(s => s.CreateAsync(
			It.IsAny<Domain.Core.Receipt>(),
			It.IsAny<List<Domain.Core.Transaction>>(),
			It.IsAny<List<Domain.Core.ReceiptItem>>(),
			It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WithTransactionsMatchingTaxOnly_NoItems_DelegatesToService()
	{
		// Arrange: no items, so ExpectedTotal = TaxAmount ($10); transaction matches
		Domain.Core.Receipt receipt = new(Guid.NewGuid(), "Test", DateOnly.FromDateTime(DateTime.Now), new Money(10));
		List<Domain.Core.Transaction> transactions =
		[
			new Domain.Core.Transaction(Guid.NewGuid(), new Money(10), DateOnly.FromDateTime(DateTime.Now))
		];
		CreateCompleteReceiptResult expectedResult = new(receipt, transactions, []);

		_mockService.Setup(s => s.CreateAsync(
				It.IsAny<Domain.Core.Receipt>(),
				It.IsAny<List<Domain.Core.Transaction>>(),
				It.IsAny<List<Domain.Core.ReceiptItem>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		CreateCompleteReceiptCommand command = new(receipt, transactions, []);

		// Act
		CreateCompleteReceiptResult result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expectedResult);
	}

	[Fact]
	public async Task Handle_WithItemsButNoTransactions_DelegatesToService()
	{
		// Arrange: items but no transactions — balance check skipped since no transactions
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		List<Domain.Core.ReceiptItem> items = ReceiptItemGenerator.GenerateList(2);
		CreateCompleteReceiptResult expectedResult = new(receipt, [], items);

		_mockService.Setup(s => s.CreateAsync(
				It.IsAny<Domain.Core.Receipt>(),
				It.IsAny<List<Domain.Core.Transaction>>(),
				It.IsAny<List<Domain.Core.ReceiptItem>>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedResult);

		CreateCompleteReceiptCommand command = new(receipt, [], items);

		// Act
		CreateCompleteReceiptResult result = await _handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expectedResult);
	}
}
