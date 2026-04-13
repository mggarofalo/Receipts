using Common;
using Domain;
using Domain.Aggregates;
using Domain.Core;

namespace Domain.Tests.Aggregates;

public class TripValidationTests
{
	private static ReceiptWithItems CreateReceiptWithItems(decimal expectedTotal)
	{
		// Build receipt + items that sum to expectedTotal
		Receipt receipt = new(
			Guid.NewGuid(),
			"Test Store",
			DateOnly.FromDateTime(DateTime.Now),
			new Money(0m)); // zero tax for simplicity

		ReceiptItem item = new(
			Guid.NewGuid(),
			"ITEM001",
			"Test Item",
			1,
			new Money(expectedTotal),
			new Money(expectedTotal),
			"Category",
			"Subcategory");

		return new ReceiptWithItems
		{
			Receipt = receipt,
			Items = [item],
			Adjustments = []
		};
	}

	private static TransactionAccount CreateTransactionAccount(decimal amount, DateOnly? date = null)
	{
		Transaction transaction = new(
			Guid.NewGuid(),
			new Money(amount),
			date ?? DateOnly.FromDateTime(DateTime.Now));

		Card account = new(
			Guid.NewGuid(),
			"ACC001",
			"Test Account",
			true);

		return new TransactionAccount
		{
			Transaction = transaction,
			Account = account
		};
	}

	[Fact]
	public void Validate_BalancedTrip_NoErrors()
	{
		// Arrange: expected=$100, transaction=$100
		Trip trip = new()
		{
			Receipt = CreateReceiptWithItems(100.00m),
			Transactions = [CreateTransactionAccount(100.00m)]
		};

		// Act
		List<string> errors = trip.Validate();

		// Assert
		Assert.Empty(errors);
	}

	[Fact]
	public void Validate_UnbalancedTrip_ReturnsError()
	{
		// Arrange: expected=$100, transaction=$90
		Trip trip = new()
		{
			Receipt = CreateReceiptWithItems(100.00m),
			Transactions = [CreateTransactionAccount(90.00m)]
		};

		// Act
		List<string> errors = trip.Validate();

		// Assert
		Assert.Single(errors);
		Assert.Contains("Balance equation violated", errors[0]);
	}

	[Fact]
	public void Validate_NoTransactions_NoErrors()
	{
		// Arrange: no transactions means skip balance check
		Trip trip = new()
		{
			Receipt = CreateReceiptWithItems(100.00m),
			Transactions = []
		};

		// Act
		List<string> errors = trip.Validate();

		// Assert
		Assert.Empty(errors);
	}

	[Fact]
	public void Validate_MultipleTransactions_SumsCorrectly()
	{
		// Arrange: expected=$100, transactions=$60+$40=$100
		Trip trip = new()
		{
			Receipt = CreateReceiptWithItems(100.00m),
			Transactions =
			[
				CreateTransactionAccount(60.00m),
				CreateTransactionAccount(40.00m)
			]
		};

		// Act
		List<string> errors = trip.Validate();

		// Assert
		Assert.Empty(errors);
	}

	[Fact]
	public void TransactionTotal_ComputedFromTransactions()
	{
		// Arrange
		Trip trip = new()
		{
			Receipt = CreateReceiptWithItems(100.00m),
			Transactions =
			[
				CreateTransactionAccount(30.00m),
				CreateTransactionAccount(70.00m)
			]
		};

		// Assert
		Assert.Equal(100.00m, trip.TransactionTotal.Amount);
	}

	[Fact]
	public void GetWarnings_TransactionBeforeReceiptDate_ReturnsWarning()
	{
		// Arrange: receipt is today, transaction is yesterday
		DateOnly receiptDate = DateOnly.FromDateTime(DateTime.Now);
		DateOnly transactionDate = receiptDate.AddDays(-1);

		Receipt receipt = new(
			Guid.NewGuid(),
			"Test Store",
			receiptDate,
			new Money(0m));

		ReceiptItem item = new(
			Guid.NewGuid(),
			"ITEM001",
			"Test Item",
			1,
			new Money(50.00m),
			new Money(50.00m),
			"Category",
			"Subcategory");

		ReceiptWithItems rwi = new()
		{
			Receipt = receipt,
			Items = [item],
			Adjustments = []
		};

		Trip trip = new()
		{
			Receipt = rwi,
			Transactions = [CreateTransactionAccount(50.00m, transactionDate)]
		};

		// Act
		List<ValidationWarning> warnings = trip.GetWarnings();

		// Assert
		Assert.Contains(warnings, w =>
			w.Property == "Transaction.Date" &&
			w.Severity == ValidationWarningSeverity.Warning);
	}

	[Fact]
	public void GetWarnings_TransactionOnReceiptDate_NoDateWarning()
	{
		// Arrange
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);

		Receipt receipt = new(
			Guid.NewGuid(),
			"Test Store",
			date,
			new Money(1.00m));

		ReceiptItem item = new(
			Guid.NewGuid(),
			"ITEM001",
			"Test Item",
			1,
			new Money(10.00m),
			new Money(10.00m),
			"Category",
			"Subcategory");

		ReceiptWithItems rwi = new()
		{
			Receipt = receipt,
			Items = [item],
			Adjustments = []
		};

		Trip trip = new()
		{
			Receipt = rwi,
			Transactions = [CreateTransactionAccount(11.00m, date)]
		};

		// Act
		List<ValidationWarning> warnings = trip.GetWarnings();

		// Assert: no date warning (may have other warnings from receipt)
		Assert.DoesNotContain(warnings, w => w.Property == "Transaction.Date");
	}

	[Fact]
	public void GetWarnings_TransactionAfterReceiptDate_NoDateWarning()
	{
		// Arrange: receipt is 2 days ago, transaction is 1 day ago (after receipt)
		DateOnly receiptDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-2));
		DateOnly transactionDate = receiptDate.AddDays(1);

		Receipt receipt = new(
			Guid.NewGuid(),
			"Test Store",
			receiptDate,
			new Money(1.00m));

		ReceiptItem item = new(
			Guid.NewGuid(),
			"ITEM001",
			"Test Item",
			1,
			new Money(10.00m),
			new Money(10.00m),
			"Category",
			"Subcategory");

		ReceiptWithItems rwi = new()
		{
			Receipt = receipt,
			Items = [item],
			Adjustments = []
		};

		Trip trip = new()
		{
			Receipt = rwi,
			Transactions = [CreateTransactionAccount(11.00m, transactionDate)]
		};

		// Act
		List<ValidationWarning> warnings = trip.GetWarnings();

		// Assert
		Assert.DoesNotContain(warnings, w => w.Property == "Transaction.Date");
	}

	[Fact]
	public void GetWarnings_IncludesReceiptWarnings()
	{
		// Arrange: receipt with 30% tax rate should trigger warning
		Receipt receipt = new(
			Guid.NewGuid(),
			"Test Store",
			DateOnly.FromDateTime(DateTime.Now),
			new Money(3.00m));

		ReceiptItem item = new(
			Guid.NewGuid(),
			"ITEM001",
			"Test Item",
			1,
			new Money(10.00m),
			new Money(10.00m),
			"Category",
			"Subcategory");

		ReceiptWithItems rwi = new()
		{
			Receipt = receipt,
			Items = [item],
			Adjustments = []
		};

		Trip trip = new()
		{
			Receipt = rwi,
			Transactions = []
		};

		// Act
		List<ValidationWarning> warnings = trip.GetWarnings();

		// Assert: includes receipt-level warnings (high tax rate)
		Assert.Contains(warnings, w => w.Property == nameof(Receipt.TaxAmount));
	}
}
