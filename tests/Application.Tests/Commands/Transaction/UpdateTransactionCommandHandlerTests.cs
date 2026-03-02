using Application.Commands.Transaction.Update;
using Application.Interfaces.Services;
using Common;
using Domain;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace Application.Tests.Commands.Transaction;

public class UpdateTransactionCommandHandlerTests
{
	private readonly Mock<ITransactionService> _transactionService = new();
	private readonly Mock<IReceiptService> _receiptService = new();
	private readonly Mock<IReceiptItemService> _receiptItemService = new();
	private readonly Mock<IAdjustmentService> _adjustmentService = new();

	private UpdateTransactionCommandHandler CreateHandler() =>
		new(_transactionService.Object, _receiptService.Object,
			_receiptItemService.Object, _adjustmentService.Object);

	private void SetupBalancedData(Guid receiptId, List<Domain.Core.Transaction> existingTransactions)
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
			.ReturnsAsync(existingTransactions);
		_transactionService.Setup(s => s.UpdateAsync(
				It.IsAny<List<Domain.Core.Transaction>>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);
	}

	[Fact]
	public async Task Handle_BalancedTransactions_ReturnsTrue()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid txId = Guid.NewGuid();
		// Existing transaction matches what we're updating (same ID)
		Domain.Core.Transaction existing = new(txId, new Money(15), DateOnly.FromDateTime(DateTime.Now));
		SetupBalancedData(receiptId, [existing]);

		// Update with same amount → still balanced at $15
		List<Domain.Core.Transaction> updated = [new(txId, new Money(15), DateOnly.FromDateTime(DateTime.Now))];

		UpdateTransactionCommandHandler handler = CreateHandler();
		UpdateTransactionCommand command = new(updated, receiptId, Guid.NewGuid());

		// Act
		bool result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public async Task Handle_UnbalancedTransactions_ThrowsValidationException()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid txId = Guid.NewGuid();
		Domain.Core.Transaction existing = new(txId, new Money(15), DateOnly.FromDateTime(DateTime.Now));
		SetupBalancedData(receiptId, [existing]);

		// Update to $100 → unbalanced (ExpectedTotal is $15)
		List<Domain.Core.Transaction> updated = [new(txId, new Money(100), DateOnly.FromDateTime(DateTime.Now))];

		UpdateTransactionCommandHandler handler = CreateHandler();
		UpdateTransactionCommand command = new(updated, receiptId, Guid.NewGuid());

		// Act
		Func<Task> act = () => handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ValidationException>();
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

		Guid txId = Guid.NewGuid();
		List<Domain.Core.Transaction> updated = [new(txId, new Money(15), DateOnly.FromDateTime(DateTime.Now))];

		UpdateTransactionCommandHandler handler = CreateHandler();
		UpdateTransactionCommand command = new(updated, receiptId, Guid.NewGuid());

		// Act
		Func<Task> act = () => handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>();
	}
}
