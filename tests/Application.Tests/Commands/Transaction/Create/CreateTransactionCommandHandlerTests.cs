using Application.Commands.Transaction.Create;
using Application.Interfaces.Services;
using Common;
using Domain;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace Application.Tests.Commands.Transaction.Create;

public class CreateTransactionCommandHandlerTests
{
	private readonly Mock<ITransactionService> _transactionService = new();
	private readonly Mock<IReceiptService> _receiptService = new();
	private readonly Mock<IReceiptItemService> _receiptItemService = new();
	private readonly Mock<IAdjustmentService> _adjustmentService = new();

	private CreateTransactionCommandHandler CreateHandler() =>
		new(_transactionService.Object, _receiptService.Object,
			_receiptItemService.Object, _adjustmentService.Object);

	private void SetupReceiptData(Guid receiptId, List<Domain.Core.Transaction>? existingTransactions = null)
	{
		// Receipt: TaxAmount = $10
		Domain.Core.Receipt receipt = new(Guid.NewGuid(), "Test", DateOnly.FromDateTime(DateTime.Now), new Money(10), "desc");

		// Item: qty=1, unitPrice=$5, totalAmount=$5 → Subtotal = $5
		Domain.Core.ReceiptItem item = new(Guid.NewGuid(), "CODE", "Item", 1, new Money(5), new Money(5), "Cat", "Sub", PricingMode.Quantity);

		// ExpectedTotal = Subtotal($5) + TaxAmount($10) + Adjustments($0) = $15

		_receiptService.Setup(s => s.GetByIdAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(receipt);
		_receiptItemService.Setup(s => s.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync([item]);
		_adjustmentService.Setup(s => s.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);
		_transactionService.Setup(s => s.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(existingTransactions ?? []);
	}

	[Fact]
	public async Task Handle_SingleGroupBalanced_ReturnsCreatedTransactions()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		List<Domain.Core.Transaction> input =
		[
			new(Guid.NewGuid(), new Money(15), DateOnly.FromDateTime(DateTime.Now)) { AccountId = accountId }
		];
		SetupReceiptData(receiptId);

		_transactionService.Setup(s => s.CreateAsync(
				It.IsAny<List<Domain.Core.Transaction>>(), receiptId, accountId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(input);

		CreateTransactionCommandHandler handler = CreateHandler();
		CreateTransactionCommand command = new(input, receiptId);

		// Act
		List<Domain.Core.Transaction> result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().HaveCount(1);
		result[0].Amount.Amount.Should().Be(15);
	}

	[Fact]
	public async Task Handle_MultipleGroupsBalanced_ReturnsAllCreatedTransactions()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId1 = Guid.NewGuid();
		Guid accountId2 = Guid.NewGuid();

		Domain.Core.Transaction tx1 = new(Guid.NewGuid(), new Money(10), DateOnly.FromDateTime(DateTime.Now)) { AccountId = accountId1 };
		Domain.Core.Transaction tx2 = new(Guid.NewGuid(), new Money(5), DateOnly.FromDateTime(DateTime.Now)) { AccountId = accountId2 };
		List<Domain.Core.Transaction> input = [tx1, tx2];

		SetupReceiptData(receiptId);

		List<Domain.Core.Transaction> group1Result = [tx1];
		List<Domain.Core.Transaction> group2Result = [tx2];

		_transactionService.Setup(s => s.CreateAsync(
				It.IsAny<List<Domain.Core.Transaction>>(), receiptId, accountId1, It.IsAny<CancellationToken>()))
			.ReturnsAsync(group1Result);
		_transactionService.Setup(s => s.CreateAsync(
				It.IsAny<List<Domain.Core.Transaction>>(), receiptId, accountId2, It.IsAny<CancellationToken>()))
			.ReturnsAsync(group2Result);

		CreateTransactionCommandHandler handler = CreateHandler();
		CreateTransactionCommand command = new(input, receiptId);

		// Act
		List<Domain.Core.Transaction> result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().HaveCount(2);
		result.Sum(t => t.Amount.Amount).Should().Be(15);
		_transactionService.Verify(s => s.CreateAsync(
			It.IsAny<List<Domain.Core.Transaction>>(), receiptId, accountId1, It.IsAny<CancellationToken>()), Times.Once);
		_transactionService.Verify(s => s.CreateAsync(
			It.IsAny<List<Domain.Core.Transaction>>(), receiptId, accountId2, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async Task Handle_UnbalancedTransactions_ThrowsValidationExceptionAndNeverPersists()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId1 = Guid.NewGuid();
		Guid accountId2 = Guid.NewGuid();

		// Total = $100 + $50 = $150 ≠ ExpectedTotal of $15
		List<Domain.Core.Transaction> input =
		[
			new(Guid.NewGuid(), new Money(100), DateOnly.FromDateTime(DateTime.Now)) { AccountId = accountId1 },
			new(Guid.NewGuid(), new Money(50), DateOnly.FromDateTime(DateTime.Now)) { AccountId = accountId2 }
		];
		SetupReceiptData(receiptId);

		CreateTransactionCommandHandler handler = CreateHandler();
		CreateTransactionCommand command = new(input, receiptId);

		// Act
		Func<Task> act = () => handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>();
		_transactionService.Verify(s => s.CreateAsync(
			It.IsAny<List<Domain.Core.Transaction>>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_ReceiptNotFound_ThrowsInvalidOperationException()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		_receiptService.Setup(s => s.GetByIdAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync((Domain.Core.Receipt?)null);
		_receiptItemService.Setup(s => s.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);
		_adjustmentService.Setup(s => s.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);
		_transactionService.Setup(s => s.GetByReceiptIdAsync(receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync([]);

		List<Domain.Core.Transaction> input =
		[
			new(Guid.NewGuid(), new Money(15), DateOnly.FromDateTime(DateTime.Now)) { AccountId = Guid.NewGuid() }
		];

		CreateTransactionCommandHandler handler = CreateHandler();
		CreateTransactionCommand command = new(input, receiptId);

		// Act
		Func<Task> act = () => handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>();
	}
}
