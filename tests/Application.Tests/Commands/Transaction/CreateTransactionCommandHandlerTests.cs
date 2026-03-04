using Application.Commands.Transaction.Create;
using Application.Interfaces.Services;
using Common;
using Domain;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace Application.Tests.Commands.Transaction;

public class CreateTransactionCommandHandlerTests
{
	private readonly Mock<ITransactionService> _transactionService = new();
	private readonly Mock<IReceiptService> _receiptService = new();
	private readonly Mock<IReceiptItemService> _receiptItemService = new();
	private readonly Mock<IAdjustmentService> _adjustmentService = new();

	private CreateTransactionCommandHandler CreateHandler() =>
		new(_transactionService.Object, _receiptService.Object,
			_receiptItemService.Object, _adjustmentService.Object);

	private void SetupBalancedData(Guid receiptId, List<Domain.Core.Transaction> transactions)
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
			.ReturnsAsync([]);
		_transactionService.Setup(s => s.CreateAsync(
				It.IsAny<List<Domain.Core.Transaction>>(), receiptId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(transactions);
	}

	[Fact]
	public async Task Handle_BalancedTransactions_ReturnsCreatedTransactions()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		// Single transaction matching ExpectedTotal of $15
		List<Domain.Core.Transaction> input =
		[
			new(Guid.NewGuid(), new Money(15), DateOnly.FromDateTime(DateTime.Now))
		];
		SetupBalancedData(receiptId, input);

		CreateTransactionCommandHandler handler = CreateHandler();
		CreateTransactionCommand command = new(input, receiptId, accountId);

		// Act
		List<Domain.Core.Transaction> result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.Should().HaveCount(1);
		result[0].Amount.Amount.Should().Be(15);
	}

	[Fact]
	public async Task Handle_UnbalancedTransactions_ThrowsValidationException()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		// Transaction $100 does not match ExpectedTotal of $15
		List<Domain.Core.Transaction> input =
		[
			new(Guid.NewGuid(), new Money(100), DateOnly.FromDateTime(DateTime.Now))
		];
		SetupBalancedData(receiptId, input);

		CreateTransactionCommandHandler handler = CreateHandler();
		CreateTransactionCommand command = new(input, receiptId, accountId);

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

		List<Domain.Core.Transaction> input =
		[
			new(Guid.NewGuid(), new Money(15), DateOnly.FromDateTime(DateTime.Now))
		];

		CreateTransactionCommandHandler handler = CreateHandler();
		CreateTransactionCommand command = new(input, receiptId, Guid.NewGuid());

		// Act — accountId doesn't matter here since handler throws before reaching CreateAsync
		Func<Task> act = () => handler.Handle(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>();
	}
}
