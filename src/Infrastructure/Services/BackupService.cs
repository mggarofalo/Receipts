using Application.Interfaces.Services;
using Infrastructure.Entities.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class BackupService(
	IDbContextFactory<ApplicationDbContext> dbContextFactory,
	ILogger<BackupService> logger) : IBackupService
{
	public async Task<string> ExportToSqliteAsync(CancellationToken cancellationToken = default)
	{
		string tempPath = Path.Combine(Path.GetTempPath(), $"receipts-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.db");
		logger.LogInformation("Starting SQLite export to {Path}", tempPath);

		await using ApplicationDbContext source = await dbContextFactory.CreateDbContextAsync(cancellationToken);

		string connectionString = $"Data Source={tempPath}";
		await using SqliteConnection sqlite = new(connectionString);
		await sqlite.OpenAsync(cancellationToken);

		await using SqliteTransaction transaction = (SqliteTransaction)await sqlite.BeginTransactionAsync(cancellationToken);

		try
		{
			await CreateSchemaAsync(sqlite, cancellationToken);
			await ExportCardsAsync(source, sqlite, cancellationToken);
			await ExportCategoriesAsync(source, sqlite, cancellationToken);
			await ExportSubcategoriesAsync(source, sqlite, cancellationToken);
			await ExportItemTemplatesAsync(source, sqlite, cancellationToken);
			await ExportReceiptsAsync(source, sqlite, cancellationToken);
			await ExportReceiptItemsAsync(source, sqlite, cancellationToken);
			await ExportTransactionsAsync(source, sqlite, cancellationToken);
			await ExportAdjustmentsAsync(source, sqlite, cancellationToken);
			await WriteMetadataAsync(sqlite, cancellationToken);

			await transaction.CommitAsync(cancellationToken);
		}
		catch
		{
			await transaction.RollbackAsync(cancellationToken);
			if (File.Exists(tempPath))
			{
				File.Delete(tempPath);
			}
			throw;
		}

		logger.LogInformation("SQLite export completed: {Path}", tempPath);
		return tempPath;
	}

	internal static async Task CreateSchemaAsync(SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		string[] ddl =
		[
			"""
			CREATE TABLE backup_metadata (
				key TEXT NOT NULL PRIMARY KEY,
				value TEXT NOT NULL
			)
			""",
			"""
			CREATE TABLE cards (
				id TEXT NOT NULL PRIMARY KEY,
				card_code TEXT NOT NULL,
				name TEXT NOT NULL,
				is_active INTEGER NOT NULL,
				account_id TEXT NOT NULL
			)
			""",
			"""
			CREATE TABLE categories (
				id TEXT NOT NULL PRIMARY KEY,
				name TEXT NOT NULL,
				description TEXT,
				is_active INTEGER NOT NULL
			)
			""",
			"""
			CREATE TABLE subcategories (
				id TEXT NOT NULL PRIMARY KEY,
				name TEXT NOT NULL,
				category_id TEXT NOT NULL,
				description TEXT,
				is_active INTEGER NOT NULL,
				FOREIGN KEY (category_id) REFERENCES categories(id)
			)
			""",
			"""
			CREATE TABLE item_templates (
				id TEXT NOT NULL PRIMARY KEY,
				name TEXT NOT NULL,
				default_category TEXT,
				default_subcategory TEXT,
				default_unit_price TEXT,
				default_unit_price_currency TEXT,
				default_pricing_mode TEXT,
				default_item_code TEXT,
				description TEXT
			)
			""",
			"""
			CREATE TABLE receipts (
				id TEXT NOT NULL PRIMARY KEY,
				location TEXT NOT NULL,
				date TEXT NOT NULL,
				tax_amount TEXT NOT NULL,
				tax_amount_currency TEXT NOT NULL
			)
			""",
			"""
			CREATE TABLE receipt_items (
				id TEXT NOT NULL PRIMARY KEY,
				receipt_id TEXT NOT NULL,
				receipt_item_code TEXT,
				description TEXT NOT NULL,
				quantity TEXT NOT NULL,
				unit_price TEXT NOT NULL,
				unit_price_currency TEXT NOT NULL,
				total_amount TEXT NOT NULL,
				total_amount_currency TEXT NOT NULL,
				category TEXT NOT NULL,
				subcategory TEXT,
				pricing_mode TEXT NOT NULL,
				FOREIGN KEY (receipt_id) REFERENCES receipts(id)
			)
			""",
			"""
			CREATE TABLE transactions (
				id TEXT NOT NULL PRIMARY KEY,
				receipt_id TEXT NOT NULL,
				card_id TEXT NOT NULL,
				amount TEXT NOT NULL,
				amount_currency TEXT NOT NULL,
				date TEXT NOT NULL,
				FOREIGN KEY (receipt_id) REFERENCES receipts(id),
				FOREIGN KEY (card_id) REFERENCES cards(id)
			)
			""",
			"""
			CREATE TABLE adjustments (
				id TEXT NOT NULL PRIMARY KEY,
				receipt_id TEXT NOT NULL,
				type TEXT NOT NULL,
				amount TEXT NOT NULL,
				amount_currency TEXT NOT NULL,
				description TEXT,
				FOREIGN KEY (receipt_id) REFERENCES receipts(id)
			)
			""",
		];

		foreach (string sql in ddl)
		{
			await using SqliteCommand cmd = sqlite.CreateCommand();
			cmd.CommandText = sql;
			await cmd.ExecuteNonQueryAsync(cancellationToken);
		}
	}

	private static async Task ExportCardsAsync(ApplicationDbContext source, SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		List<CardEntity> cards = await source.Cards.AsNoTracking().ToListAsync(cancellationToken);

		const string sql = "INSERT INTO cards (id, card_code, name, is_active, account_id) VALUES ($id, $code, $name, $active, $accountId)";
		foreach (CardEntity card in cards)
		{
			await using SqliteCommand cmd = sqlite.CreateCommand();
			cmd.CommandText = sql;
			cmd.Parameters.AddWithValue("$id", card.Id.ToString());
			cmd.Parameters.AddWithValue("$code", card.CardCode);
			cmd.Parameters.AddWithValue("$name", card.Name);
			cmd.Parameters.AddWithValue("$active", card.IsActive ? 1 : 0);
			cmd.Parameters.AddWithValue("$accountId", card.AccountId.ToString());
			await cmd.ExecuteNonQueryAsync(cancellationToken);
		}
	}

	private static async Task ExportCategoriesAsync(ApplicationDbContext source, SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		List<CategoryEntity> categories = await source.Categories
			.AsNoTracking()
			.Where(c => c.DeletedAt == null)
			.ToListAsync(cancellationToken);

		const string sql = "INSERT INTO categories (id, name, description, is_active) VALUES ($id, $name, $desc, $active)";
		foreach (CategoryEntity category in categories)
		{
			await using SqliteCommand cmd = sqlite.CreateCommand();
			cmd.CommandText = sql;
			cmd.Parameters.AddWithValue("$id", category.Id.ToString());
			cmd.Parameters.AddWithValue("$name", category.Name);
			cmd.Parameters.AddWithValue("$desc", (object?)category.Description ?? DBNull.Value);
			cmd.Parameters.AddWithValue("$active", category.IsActive ? 1 : 0);
			await cmd.ExecuteNonQueryAsync(cancellationToken);
		}
	}

	private static async Task ExportSubcategoriesAsync(ApplicationDbContext source, SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		List<SubcategoryEntity> subcategories = await source.Subcategories
			.AsNoTracking()
			.Where(s => s.DeletedAt == null)
			.ToListAsync(cancellationToken);

		const string sql = "INSERT INTO subcategories (id, name, category_id, description, is_active) VALUES ($id, $name, $catId, $desc, $active)";
		foreach (SubcategoryEntity sub in subcategories)
		{
			await using SqliteCommand cmd = sqlite.CreateCommand();
			cmd.CommandText = sql;
			cmd.Parameters.AddWithValue("$id", sub.Id.ToString());
			cmd.Parameters.AddWithValue("$name", sub.Name);
			cmd.Parameters.AddWithValue("$catId", sub.CategoryId.ToString());
			cmd.Parameters.AddWithValue("$desc", (object?)sub.Description ?? DBNull.Value);
			cmd.Parameters.AddWithValue("$active", sub.IsActive ? 1 : 0);
			await cmd.ExecuteNonQueryAsync(cancellationToken);
		}
	}

	private static async Task ExportItemTemplatesAsync(ApplicationDbContext source, SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		List<ItemTemplateEntity> templates = await source.ItemTemplates
			.AsNoTracking()
			.Where(t => t.DeletedAt == null)
			.ToListAsync(cancellationToken);

		const string sql = """
			INSERT INTO item_templates (id, name, default_category, default_subcategory,
				default_unit_price, default_unit_price_currency, default_pricing_mode,
				default_item_code, description)
			VALUES ($id, $name, $cat, $subcat, $price, $priceCurrency, $pricingMode, $itemCode, $desc)
			""";

		foreach (ItemTemplateEntity template in templates)
		{
			await using SqliteCommand cmd = sqlite.CreateCommand();
			cmd.CommandText = sql;
			cmd.Parameters.AddWithValue("$id", template.Id.ToString());
			cmd.Parameters.AddWithValue("$name", template.Name);
			cmd.Parameters.AddWithValue("$cat", (object?)template.DefaultCategory ?? DBNull.Value);
			cmd.Parameters.AddWithValue("$subcat", (object?)template.DefaultSubcategory ?? DBNull.Value);
			cmd.Parameters.AddWithValue("$price", template.DefaultUnitPrice.HasValue ? template.DefaultUnitPrice.Value.ToString("G") : DBNull.Value);
			cmd.Parameters.AddWithValue("$priceCurrency", template.DefaultUnitPriceCurrency.HasValue ? template.DefaultUnitPriceCurrency.Value.ToString() : DBNull.Value);
			cmd.Parameters.AddWithValue("$pricingMode", (object?)template.DefaultPricingMode ?? DBNull.Value);
			cmd.Parameters.AddWithValue("$itemCode", (object?)template.DefaultItemCode ?? DBNull.Value);
			cmd.Parameters.AddWithValue("$desc", (object?)template.Description ?? DBNull.Value);
			await cmd.ExecuteNonQueryAsync(cancellationToken);
		}
	}

	private static async Task ExportReceiptsAsync(ApplicationDbContext source, SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		List<ReceiptEntity> receipts = await source.Receipts
			.AsNoTracking()
			.Where(r => r.DeletedAt == null)
			.ToListAsync(cancellationToken);

		const string sql = "INSERT INTO receipts (id, location, date, tax_amount, tax_amount_currency) VALUES ($id, $loc, $date, $tax, $taxCurrency)";
		foreach (ReceiptEntity receipt in receipts)
		{
			await using SqliteCommand cmd = sqlite.CreateCommand();
			cmd.CommandText = sql;
			cmd.Parameters.AddWithValue("$id", receipt.Id.ToString());
			cmd.Parameters.AddWithValue("$loc", receipt.Location);
			cmd.Parameters.AddWithValue("$date", receipt.Date.ToString("O"));
			cmd.Parameters.AddWithValue("$tax", receipt.TaxAmount.ToString("G"));
			cmd.Parameters.AddWithValue("$taxCurrency", receipt.TaxAmountCurrency.ToString());
			await cmd.ExecuteNonQueryAsync(cancellationToken);
		}
	}

	private static async Task ExportReceiptItemsAsync(ApplicationDbContext source, SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> items = await source.ReceiptItems
			.AsNoTracking()
			.Where(i => i.DeletedAt == null)
			.ToListAsync(cancellationToken);

		const string sql = """
			INSERT INTO receipt_items (id, receipt_id, receipt_item_code, description, quantity,
				unit_price, unit_price_currency, total_amount, total_amount_currency,
				category, subcategory, pricing_mode)
			VALUES ($id, $receiptId, $itemCode, $desc, $qty, $unitPrice, $unitPriceCurrency,
				$totalAmt, $totalAmtCurrency, $cat, $subcat, $pricingMode)
			""";

		foreach (ReceiptItemEntity item in items)
		{
			await using SqliteCommand cmd = sqlite.CreateCommand();
			cmd.CommandText = sql;
			cmd.Parameters.AddWithValue("$id", item.Id.ToString());
			cmd.Parameters.AddWithValue("$receiptId", item.ReceiptId.ToString());
			cmd.Parameters.AddWithValue("$itemCode", (object?)item.ReceiptItemCode ?? DBNull.Value);
			cmd.Parameters.AddWithValue("$desc", item.Description);
			cmd.Parameters.AddWithValue("$qty", item.Quantity.ToString("G"));
			cmd.Parameters.AddWithValue("$unitPrice", item.UnitPrice.ToString("G"));
			cmd.Parameters.AddWithValue("$unitPriceCurrency", item.UnitPriceCurrency.ToString());
			cmd.Parameters.AddWithValue("$totalAmt", item.TotalAmount.ToString("G"));
			cmd.Parameters.AddWithValue("$totalAmtCurrency", item.TotalAmountCurrency.ToString());
			cmd.Parameters.AddWithValue("$cat", item.Category);
			cmd.Parameters.AddWithValue("$subcat", (object?)item.Subcategory ?? DBNull.Value);
			cmd.Parameters.AddWithValue("$pricingMode", item.PricingMode.ToString());
			await cmd.ExecuteNonQueryAsync(cancellationToken);
		}
	}

	private static async Task ExportTransactionsAsync(ApplicationDbContext source, SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		List<TransactionEntity> transactions = await source.Transactions
			.AsNoTracking()
			.Where(t => t.DeletedAt == null)
			.ToListAsync(cancellationToken);

		const string sql = "INSERT INTO transactions (id, receipt_id, card_id, amount, amount_currency, date) VALUES ($id, $receiptId, $cardId, $amt, $amtCurrency, $date)";
		foreach (TransactionEntity txn in transactions)
		{
			await using SqliteCommand cmd = sqlite.CreateCommand();
			cmd.CommandText = sql;
			cmd.Parameters.AddWithValue("$id", txn.Id.ToString());
			cmd.Parameters.AddWithValue("$receiptId", txn.ReceiptId.ToString());
			cmd.Parameters.AddWithValue("$cardId", txn.AccountId.ToString());
			cmd.Parameters.AddWithValue("$amt", txn.Amount.ToString("G"));
			cmd.Parameters.AddWithValue("$amtCurrency", txn.AmountCurrency.ToString());
			cmd.Parameters.AddWithValue("$date", txn.Date.ToString("O"));
			await cmd.ExecuteNonQueryAsync(cancellationToken);
		}
	}

	private static async Task ExportAdjustmentsAsync(ApplicationDbContext source, SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		List<AdjustmentEntity> adjustments = await source.Adjustments
			.AsNoTracking()
			.Where(a => a.DeletedAt == null)
			.ToListAsync(cancellationToken);

		const string sql = "INSERT INTO adjustments (id, receipt_id, type, amount, amount_currency, description) VALUES ($id, $receiptId, $type, $amt, $amtCurrency, $desc)";
		foreach (AdjustmentEntity adj in adjustments)
		{
			await using SqliteCommand cmd = sqlite.CreateCommand();
			cmd.CommandText = sql;
			cmd.Parameters.AddWithValue("$id", adj.Id.ToString());
			cmd.Parameters.AddWithValue("$receiptId", adj.ReceiptId.ToString());
			cmd.Parameters.AddWithValue("$type", adj.Type.ToString());
			cmd.Parameters.AddWithValue("$amt", adj.Amount.ToString("G"));
			cmd.Parameters.AddWithValue("$amtCurrency", adj.AmountCurrency.ToString());
			cmd.Parameters.AddWithValue("$desc", (object?)adj.Description ?? DBNull.Value);
			await cmd.ExecuteNonQueryAsync(cancellationToken);
		}
	}

	private static async Task WriteMetadataAsync(SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		Dictionary<string, string> metadata = new()
		{
			["export_version"] = "3",
			["exported_at"] = DateTimeOffset.UtcNow.ToString("O"),
			["format"] = "receipts-sqlite-backup",
		};

		const string sql = "INSERT INTO backup_metadata (key, value) VALUES ($key, $value)";
		foreach (KeyValuePair<string, string> kv in metadata)
		{
			await using SqliteCommand cmd = sqlite.CreateCommand();
			cmd.CommandText = sql;
			cmd.Parameters.AddWithValue("$key", kv.Key);
			cmd.Parameters.AddWithValue("$value", kv.Value);
			await cmd.ExecuteNonQueryAsync(cancellationToken);
		}
	}
}
