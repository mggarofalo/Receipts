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
			new Domain.Core.Transaction(Guid.NewGuid(), Guid.NewGuid(), new Money(expectedTotal), DateOnly.FromDateTime(DateTime.Now))
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
			new Domain.Core.Transaction(Guid.NewGuid(), Guid.NewGuid(), new Money(15), DateOnly.FromDateTime(DateTime.Now))
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
			new Domain.Core.Transaction(Guid.NewGuid(), Guid.NewGuid(), new Money(10), DateOnly.FromDateTime(DateTime.Now))
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
	public async Task Handle_WithHalfCentRoundingDifference_DelegatesToService()
	{
		// Arrange: 3 x $1.005 floors to $3.01, but half-up rounds to $3.02.
		// A transaction of $3.02 + tax should be accepted within the 1-cent tolerance.
		decimal taxAmount = 0.25m;
		Domain.Core.Receipt receipt = new(Guid.NewGuid(), "Test", DateOnly.FromDateTime(DateTime.Now), new Money(taxAmount));

		// ReceiptItem constructor floors: Math.Floor(3 * 1.005 * 100) / 100 = $3.01
		Domain.Core.ReceiptItem item = new(
			Guid.NewGuid(), null, "Half-cent item", 3, new Money(1.005m), new Money(3.01m), "Groceries", null);

		List<Domain.Core.ReceiptItem> items = [item];

		// The item subtotal (via TotalAmount) is $3.01, so ExpectedTotal = $3.01 + $0.25 = $3.26.
		// A user whose UI half-up-rounded would see $3.02 subtotal → expected $3.27.
		// The tolerance allows this 1-cent difference.
		decimal transactionAmount = 3.02m + taxAmount; // $3.27 — 1 cent over expectedTotal of $3.26
		List<Domain.Core.Transaction> transactions =
		[
			new Domain.Core.Transaction(Guid.NewGuid(), Guid.NewGuid(), new Money(transactionAmount), DateOnly.FromDateTime(DateTime.Now))
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

		// Assert — should succeed, not throw
		result.Should().BeSameAs(expectedResult);
		_mockService.Verify(s => s.CreateAsync(
			It.IsAny<Domain.Core.Receipt>(),
			It.IsAny<List<Domain.Core.Transaction>>(),
			It.IsAny<List<Domain.Core.ReceiptItem>>(),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_WithLargeBalanceDifference_StillThrowsValidationException()
	{
		// Arrange: items total $10, tax $10, expected $20, but transactions total $22 (> 1 cent off)
		Domain.Core.Receipt receipt = new(Guid.NewGuid(), "Test", DateOnly.FromDateTime(DateTime.Now), new Money(10));
		List<Domain.Core.ReceiptItem> items = ReceiptItemGenerator.GenerateList(2); // 2 x $5 = $10 subtotal
		List<Domain.Core.Transaction> transactions =
		[
			new Domain.Core.Transaction(Guid.NewGuid(), Guid.NewGuid(), new Money(22), DateOnly.FromDateTime(DateTime.Now))
		];

		CreateCompleteReceiptCommand command = new(receipt, transactions, items);

		// Act & Assert — $22 vs $20 = $2 difference, well beyond tolerance
		await Assert.ThrowsAsync<ValidationException>(() =>
			_handler.Handle(command, CancellationToken.None));

		_mockService.Verify(s => s.CreateAsync(
			It.IsAny<Domain.Core.Receipt>(),
			It.IsAny<List<Domain.Core.Transaction>>(),
			It.IsAny<List<Domain.Core.ReceiptItem>>(),
			It.IsAny<CancellationToken>()), Times.Never);
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
