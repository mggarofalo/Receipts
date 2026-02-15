using API.Generated.Dtos;
using API.Mapping.Aggregates;
using Common;
using Domain;
using Domain.Aggregates;
using Domain.Core;

namespace Presentation.API.Tests.Mapping.Aggregates;

public class TripMapperTests
{
	private readonly TripMapper _mapper = new();

	[Fact]
	public void ToResponse_MapsReceiptWithItemsAndTransactions()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid itemId = Guid.NewGuid();
		Guid transactionId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();

		Receipt receipt = new(
			receiptId,
			"Department Store",
			new DateOnly(2025, 5, 20),
			new Money(7.89m, Currency.USD),
			"Trip to mall"
		);

		ReceiptItem item = new(
			itemId,
			"TRIP-ITEM-001",
			"Dress Shirt",
			1.0m,
			new Money(45.99m, Currency.USD),
			new Money(45.99m, Currency.USD),
			"Clothing",
			"Formal"
		);

		ReceiptWithItems receiptWithItems = new()
		{
			Receipt = receipt,
			Items = [item]
		};

		Transaction transaction = new(
			transactionId,
			new Money(53.88m, Currency.USD),
			new DateOnly(2025, 5, 20)
		);

		Account account = new(
			accountId,
			"DEBIT-001",
			"Debit Card",
			true
		);

		TransactionAccount transactionAccount = new()
		{
			Transaction = transaction,
			Account = account
		};

		Trip trip = new()
		{
			Receipt = receiptWithItems,
			Transactions = [transactionAccount]
		};

		// Act
		TripResponse actual = _mapper.ToResponse(trip);

		// Assert — Receipt
		Assert.Equal(receiptId, actual.Receipt.Receipt.Id);
		Assert.Equal("Department Store", actual.Receipt.Receipt.Location);
		Assert.Equal(new DateOnly(2025, 5, 20), actual.Receipt.Receipt.Date);
		Assert.Equal((double)7.89m, actual.Receipt.Receipt.TaxAmount);
		Assert.Equal("Trip to mall", actual.Receipt.Receipt.Description);

		// Assert — Receipt Items
		Assert.Single(actual.Receipt.Items);
		ReceiptItemResponse actualItem = actual.Receipt.Items.First();
		Assert.Equal(itemId, actualItem.Id);
		Assert.Equal("TRIP-ITEM-001", actualItem.ReceiptItemCode);
		Assert.Equal("Dress Shirt", actualItem.Description);
		Assert.Equal((double)1.0m, actualItem.Quantity);
		Assert.Equal((double)45.99m, actualItem.UnitPrice);
		Assert.Equal("Clothing", actualItem.Category);
		Assert.Equal("Formal", actualItem.Subcategory);

		// Assert — Transactions
		Assert.Single(actual.Transactions);
		TransactionAccountResponse actualTxn = actual.Transactions.First();
		Assert.Equal(transactionId, actualTxn.Transaction.Id);
		Assert.Equal((double)53.88m, actualTxn.Transaction.Amount);
		Assert.Equal(new DateOnly(2025, 5, 20), actualTxn.Transaction.Date);

		// Assert — Account
		Assert.Equal(accountId, actualTxn.Account.Id);
		Assert.Equal("DEBIT-001", actualTxn.Account.AccountCode);
		Assert.Equal("Debit Card", actualTxn.Account.Name);
		Assert.True(actualTxn.Account.IsActive);
	}

	[Fact]
	public void ToResponse_MapsMultipleTransactions()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Guid txn1Id = Guid.NewGuid();
		Guid txn2Id = Guid.NewGuid();
		Guid acct1Id = Guid.NewGuid();
		Guid acct2Id = Guid.NewGuid();

		Receipt receipt = new(
			receiptId,
			"Gas Station",
			new DateOnly(2025, 6, 10),
			new Money(3.20m, Currency.USD)
		);

		ReceiptWithItems receiptWithItems = new()
		{
			Receipt = receipt,
			Items = []
		};

		Transaction txn1 = new(
			txn1Id,
			new Money(30.00m, Currency.USD),
			new DateOnly(2025, 6, 10)
		);

		Account acct1 = new(acct1Id, "CASH-001", "Cash", true);

		Transaction txn2 = new(
			txn2Id,
			new Money(15.45m, Currency.USD),
			new DateOnly(2025, 6, 10)
		);

		Account acct2 = new(acct2Id, "VISA-001", "Visa Card", true);

		Trip trip = new()
		{
			Receipt = receiptWithItems,
			Transactions =
			[
				new TransactionAccount { Transaction = txn1, Account = acct1 },
				new TransactionAccount { Transaction = txn2, Account = acct2 }
			]
		};

		// Act
		TripResponse actual = _mapper.ToResponse(trip);

		// Assert
		Assert.Equal(receiptId, actual.Receipt.Receipt.Id);
		Assert.Empty(actual.Receipt.Items);
		Assert.Equal(2, actual.Transactions.Count);

		List<TransactionAccountResponse> txns = actual.Transactions.ToList();
		Assert.Equal(txn1Id, txns[0].Transaction.Id);
		Assert.Equal((double)30.00m, txns[0].Transaction.Amount);
		Assert.Equal("CASH-001", txns[0].Account.AccountCode);

		Assert.Equal(txn2Id, txns[1].Transaction.Id);
		Assert.Equal((double)15.45m, txns[1].Transaction.Amount);
		Assert.Equal("VISA-001", txns[1].Account.AccountCode);
	}

	[Fact]
	public void ToResponse_MapsEmptyTransactionsList()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		Receipt receipt = new(
			receiptId,
			"Standalone Store",
			new DateOnly(2025, 7, 1),
			new Money(1.00m, Currency.USD)
		);

		Trip trip = new()
		{
			Receipt = new ReceiptWithItems { Receipt = receipt, Items = [] },
			Transactions = []
		};

		// Act
		TripResponse actual = _mapper.ToResponse(trip);

		// Assert
		Assert.Equal(receiptId, actual.Receipt.Receipt.Id);
		Assert.Empty(actual.Transactions);
	}
}
