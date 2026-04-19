using Application.Interfaces.Services;
using Application.Models;
using Common;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class BackupImportService(
	IDbContextFactory<ApplicationDbContext> contextFactory,
	ILogger<BackupImportService> logger) : IBackupImportService
{
	private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100 MB

	public async Task<BackupImportResult> ImportFromSqliteAsync(Stream sqliteStream, CancellationToken cancellationToken)
	{
		string tempPath = Path.GetTempFileName();
		try
		{
			await using (FileStream fs = File.Create(tempPath))
			{
				long totalCopied = 0;
				byte[] buffer = new byte[81920];
				int bytesRead;
				while ((bytesRead = await sqliteStream.ReadAsync(buffer, cancellationToken)) > 0)
				{
					totalCopied += bytesRead;
					if (totalCopied > MaxFileSizeBytes)
					{
						throw new InvalidOperationException($"SQLite file exceeds maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");
					}
					await fs.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
				}
			}

			return await ImportFromFileAsync(tempPath, cancellationToken);
		}
		finally
		{
			try
			{
				File.Delete(tempPath);
			}
			catch
			{
				// Best effort cleanup
			}
		}
	}

	private async Task<BackupImportResult> ImportFromFileAsync(string sqlitePath, CancellationToken cancellationToken)
	{
		ValidateSqliteFile(sqlitePath);

		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);
		await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
			await context.Database.BeginTransactionAsync(cancellationToken);

		try
		{
			await using SqliteConnection sqlite = new($"Data Source={sqlitePath};Mode=ReadOnly;Pooling=False");
			await sqlite.OpenAsync(cancellationToken);

			int exportVersion = ReadExportVersion(sqlite);

			// Import in dependency order: independent entities first, then dependent ones.
			(int accountsCreated, int accountsUpdated) = await UpsertCardsAsync(context, sqlite, exportVersion, cancellationToken);
			(int categoriesCreated, int categoriesUpdated) = await UpsertCategoriesAsync(context, sqlite, cancellationToken);
			(int subcategoriesCreated, int subcategoriesUpdated) = await UpsertSubcategoriesAsync(context, sqlite, cancellationToken);
			(int itemTemplatesCreated, int itemTemplatesUpdated) = await UpsertItemTemplatesAsync(context, sqlite, cancellationToken);
			(int receiptsCreated, int receiptsUpdated) = await UpsertReceiptsAsync(context, sqlite, cancellationToken);
			(int receiptItemsCreated, int receiptItemsUpdated) = await UpsertReceiptItemsAsync(context, sqlite, cancellationToken);
			(int transactionsCreated, int transactionsUpdated) = await UpsertTransactionsAsync(context, sqlite, exportVersion, cancellationToken);
			(int adjustmentsCreated, int adjustmentsUpdated) = await UpsertAdjustmentsAsync(context, sqlite, cancellationToken);

			await transaction.CommitAsync(cancellationToken);

			BackupImportResult result = new(
				accountsCreated, accountsUpdated,
				categoriesCreated, categoriesUpdated,
				subcategoriesCreated, subcategoriesUpdated,
				itemTemplatesCreated, itemTemplatesUpdated,
				receiptsCreated, receiptsUpdated,
				receiptItemsCreated, receiptItemsUpdated,
				transactionsCreated, transactionsUpdated,
				adjustmentsCreated, adjustmentsUpdated);

			logger.LogInformation(
				"Backup import complete: {TotalCreated} created, {TotalUpdated} updated",
				result.TotalCreated, result.TotalUpdated);

			return result;
		}
		catch
		{
			await transaction.RollbackAsync(CancellationToken.None);
			throw;
		}
	}

	private static void ValidateSqliteFile(string path)
	{
		// SQLite files start with the magic string "SQLite format 3\000"
		byte[] header = new byte[16];
		using FileStream fs = File.OpenRead(path);
		int bytesRead = fs.Read(header, 0, header.Length);
		if (bytesRead < 16)
		{
			throw new InvalidOperationException("The uploaded file is not a valid SQLite database.");
		}

		string magic = System.Text.Encoding.ASCII.GetString(header, 0, 16);
		if (magic != "SQLite format 3\0")
		{
			throw new InvalidOperationException("The uploaded file is not a valid SQLite database.");
		}
	}

	/// <summary>
	/// Clears soft-delete markers so that a restored record becomes visible again.
	/// </summary>
	private static void ClearSoftDelete(ISoftDeletable entity)
	{
		entity.DeletedAt = null;
		entity.DeletedByUserId = null;
		entity.DeletedByApiKeyId = null;
		entity.CascadeDeletedByParentId = null;
	}

	private static bool TableExists(SqliteConnection sqlite, string tableName)
	{
		using SqliteCommand cmd = sqlite.CreateCommand();
		cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@name";
		cmd.Parameters.AddWithValue("@name", tableName);
		return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
	}

	// Reads the export_version from backup_metadata; defaults to 1 for legacy backups
	// that predate version bumps or were written without metadata.
	private static int ReadExportVersion(SqliteConnection sqlite)
	{
		if (!TableExists(sqlite, "backup_metadata"))
		{
			return 1;
		}

		using SqliteCommand cmd = sqlite.CreateCommand();
		cmd.CommandText = "SELECT value FROM backup_metadata WHERE key = 'export_version'";
		object? result = cmd.ExecuteScalar();
		if (result is null || result is DBNull)
		{
			return 1;
		}

		return int.TryParse(result.ToString(), out int version) ? version : 1;
	}

	private static async Task<(int Created, int Updated)> UpsertCardsAsync(
		ApplicationDbContext context, SqliteConnection sqlite, int exportVersion, CancellationToken cancellationToken)
	{
		// v2+ writes to `cards`/`card_code`; v1 wrote to `accounts`/`account_code`.
		bool isLegacy = exportVersion < 2;
		string tableName = isLegacy ? "accounts" : "cards";
		string codeColumn = isLegacy ? "account_code" : "card_code";

		if (!TableExists(sqlite, tableName))
		{
			return (0, 0);
		}

		int created = 0, updated = 0;
		await using SqliteCommand cmd = sqlite.CreateCommand();
		cmd.CommandText = $"SELECT id, {codeColumn}, name, is_active FROM {tableName}";
		await using SqliteDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

		while (await reader.ReadAsync(cancellationToken))
		{
			Guid id = Guid.Parse(reader.GetString(0));
			string cardCode = reader.GetString(1);
			string name = reader.GetString(2);
			bool isActive = reader.GetBoolean(3);

			CardEntity? existing = await context.Cards.FindAsync([id], cancellationToken);
			if (existing is not null)
			{
				existing.CardCode = cardCode;
				existing.Name = name;
				existing.IsActive = isActive;
				updated++;
			}
			else
			{
				context.Cards.Add(new CardEntity
				{
					Id = id,
					CardCode = cardCode,
					Name = name,
					IsActive = isActive,
				});
				created++;
			}
		}

		await context.SaveChangesAsync(cancellationToken);
		return (created, updated);
	}

	private static async Task<(int Created, int Updated)> UpsertCategoriesAsync(
		ApplicationDbContext context, SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		if (!TableExists(sqlite, "categories"))
		{
			return (0, 0);
		}

		int created = 0, updated = 0;
		await using SqliteCommand cmd = sqlite.CreateCommand();
		cmd.CommandText = "SELECT id, name, description, is_active FROM categories";
		await using SqliteDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

		while (await reader.ReadAsync(cancellationToken))
		{
			Guid id = Guid.Parse(reader.GetString(0));
			string name = reader.GetString(1);
			string? description = reader.IsDBNull(2) ? null : reader.GetString(2);
			bool isActive = reader.GetBoolean(3);

			CategoryEntity? existing = await context.Categories
				.IgnoreQueryFilters()
				.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
			if (existing is not null)
			{
				existing.Name = name;
				existing.Description = description;
				existing.IsActive = isActive;
				ClearSoftDelete(existing);
				updated++;
			}
			else
			{
				context.Categories.Add(new CategoryEntity
				{
					Id = id,
					Name = name,
					Description = description,
					IsActive = isActive,
				});
				created++;
			}
		}

		await context.SaveChangesAsync(cancellationToken);
		return (created, updated);
	}

	private static async Task<(int Created, int Updated)> UpsertSubcategoriesAsync(
		ApplicationDbContext context, SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		if (!TableExists(sqlite, "subcategories"))
		{
			return (0, 0);
		}

		int created = 0, updated = 0;
		await using SqliteCommand cmd = sqlite.CreateCommand();
		cmd.CommandText = "SELECT id, name, category_id, description, is_active FROM subcategories";
		await using SqliteDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

		while (await reader.ReadAsync(cancellationToken))
		{
			Guid id = Guid.Parse(reader.GetString(0));
			string name = reader.GetString(1);
			Guid categoryId = Guid.Parse(reader.GetString(2));
			string? description = reader.IsDBNull(3) ? null : reader.GetString(3);
			bool isActive = reader.GetBoolean(4);

			SubcategoryEntity? existing = await context.Subcategories
				.IgnoreQueryFilters()
				.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
			if (existing is not null)
			{
				existing.Name = name;
				existing.CategoryId = categoryId;
				existing.Description = description;
				existing.IsActive = isActive;
				ClearSoftDelete(existing);
				updated++;
			}
			else
			{
				context.Subcategories.Add(new SubcategoryEntity
				{
					Id = id,
					Name = name,
					CategoryId = categoryId,
					Description = description,
					IsActive = isActive,
				});
				created++;
			}
		}

		await context.SaveChangesAsync(cancellationToken);
		return (created, updated);
	}

	private static async Task<(int Created, int Updated)> UpsertItemTemplatesAsync(
		ApplicationDbContext context, SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		if (!TableExists(sqlite, "item_templates"))
		{
			return (0, 0);
		}

		int created = 0, updated = 0;
		await using SqliteCommand cmd = sqlite.CreateCommand();
		cmd.CommandText = "SELECT id, name, default_category, default_subcategory, default_unit_price, default_unit_price_currency, default_pricing_mode, default_item_code, description FROM item_templates";
		await using SqliteDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

		while (await reader.ReadAsync(cancellationToken))
		{
			Guid id = Guid.Parse(reader.GetString(0));
			string name = reader.GetString(1);
			string? defaultCategory = reader.IsDBNull(2) ? null : reader.GetString(2);
			string? defaultSubcategory = reader.IsDBNull(3) ? null : reader.GetString(3);
			decimal? defaultUnitPrice = reader.IsDBNull(4) ? null : reader.GetDecimal(4);
			Currency? defaultUnitPriceCurrency = reader.IsDBNull(5) ? null : Enum.Parse<Currency>(reader.GetString(5));
			string? defaultPricingMode = reader.IsDBNull(6) ? null : reader.GetString(6);
			string? defaultItemCode = reader.IsDBNull(7) ? null : reader.GetString(7);
			string? description = reader.IsDBNull(8) ? null : reader.GetString(8);

			ItemTemplateEntity? existing = await context.ItemTemplates
				.IgnoreQueryFilters()
				.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
			if (existing is not null)
			{
				existing.Name = name;
				existing.DefaultCategory = defaultCategory;
				existing.DefaultSubcategory = defaultSubcategory;
				existing.DefaultUnitPrice = defaultUnitPrice;
				existing.DefaultUnitPriceCurrency = defaultUnitPriceCurrency;
				existing.DefaultPricingMode = defaultPricingMode;
				existing.DefaultItemCode = defaultItemCode;
				existing.Description = description;
				ClearSoftDelete(existing);
				updated++;
			}
			else
			{
				context.ItemTemplates.Add(new ItemTemplateEntity
				{
					Id = id,
					Name = name,
					DefaultCategory = defaultCategory,
					DefaultSubcategory = defaultSubcategory,
					DefaultUnitPrice = defaultUnitPrice,
					DefaultUnitPriceCurrency = defaultUnitPriceCurrency,
					DefaultPricingMode = defaultPricingMode,
					DefaultItemCode = defaultItemCode,
					Description = description,
				});
				created++;
			}
		}

		await context.SaveChangesAsync(cancellationToken);
		return (created, updated);
	}

	private static async Task<(int Created, int Updated)> UpsertReceiptsAsync(
		ApplicationDbContext context, SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		if (!TableExists(sqlite, "receipts"))
		{
			return (0, 0);
		}

		int created = 0, updated = 0;
		await using SqliteCommand cmd = sqlite.CreateCommand();
		cmd.CommandText = "SELECT id, location, date, tax_amount, tax_amount_currency FROM receipts";
		await using SqliteDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

		while (await reader.ReadAsync(cancellationToken))
		{
			Guid id = Guid.Parse(reader.GetString(0));
			string location = reader.GetString(1);
			DateOnly date = DateOnly.Parse(reader.GetString(2));
			decimal taxAmount = reader.GetDecimal(3);
			Currency taxAmountCurrency = Enum.Parse<Currency>(reader.GetString(4));

			ReceiptEntity? existing = await context.Receipts
				.IgnoreQueryFilters()
				.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
			if (existing is not null)
			{
				existing.Location = location;
				existing.Date = date;
				existing.TaxAmount = taxAmount;
				existing.TaxAmountCurrency = taxAmountCurrency;
				ClearSoftDelete(existing);
				updated++;
			}
			else
			{
				context.Receipts.Add(new ReceiptEntity
				{
					Id = id,
					Location = location,
					Date = date,
					TaxAmount = taxAmount,
					TaxAmountCurrency = taxAmountCurrency,
				});
				created++;
			}
		}

		await context.SaveChangesAsync(cancellationToken);
		return (created, updated);
	}

	private static async Task<(int Created, int Updated)> UpsertReceiptItemsAsync(
		ApplicationDbContext context, SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		if (!TableExists(sqlite, "receipt_items"))
		{
			return (0, 0);
		}

		int created = 0, updated = 0;
		await using SqliteCommand cmd = sqlite.CreateCommand();
		cmd.CommandText = "SELECT id, receipt_id, receipt_item_code, description, quantity, unit_price, unit_price_currency, total_amount, total_amount_currency, category, subcategory, pricing_mode FROM receipt_items";
		await using SqliteDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

		while (await reader.ReadAsync(cancellationToken))
		{
			Guid id = Guid.Parse(reader.GetString(0));
			Guid receiptId = Guid.Parse(reader.GetString(1));
			string? receiptItemCode = reader.IsDBNull(2) ? null : reader.GetString(2);
			string description = reader.GetString(3);
			decimal quantity = reader.GetDecimal(4);
			decimal unitPrice = reader.GetDecimal(5);
			Currency unitPriceCurrency = Enum.Parse<Currency>(reader.GetString(6));
			decimal totalAmount = reader.GetDecimal(7);
			Currency totalAmountCurrency = Enum.Parse<Currency>(reader.GetString(8));
			string category = reader.GetString(9);
			string? subcategory = reader.IsDBNull(10) ? null : reader.GetString(10);
			PricingMode pricingMode = Enum.Parse<PricingMode>(reader.GetString(11));

			ReceiptItemEntity? existing = await context.ReceiptItems
				.IgnoreQueryFilters()
				.FirstOrDefaultAsync(ri => ri.Id == id, cancellationToken);
			if (existing is not null)
			{
				existing.ReceiptId = receiptId;
				existing.ReceiptItemCode = receiptItemCode;
				existing.Description = description;
				existing.Quantity = quantity;
				existing.UnitPrice = unitPrice;
				existing.UnitPriceCurrency = unitPriceCurrency;
				existing.TotalAmount = totalAmount;
				existing.TotalAmountCurrency = totalAmountCurrency;
				existing.Category = category;
				existing.Subcategory = subcategory;
				existing.PricingMode = pricingMode;
				ClearSoftDelete(existing);
				updated++;
			}
			else
			{
				context.ReceiptItems.Add(new ReceiptItemEntity
				{
					Id = id,
					ReceiptId = receiptId,
					ReceiptItemCode = receiptItemCode,
					Description = description,
					Quantity = quantity,
					UnitPrice = unitPrice,
					UnitPriceCurrency = unitPriceCurrency,
					TotalAmount = totalAmount,
					TotalAmountCurrency = totalAmountCurrency,
					Category = category,
					Subcategory = subcategory,
					PricingMode = pricingMode,
				});
				created++;
			}
		}

		await context.SaveChangesAsync(cancellationToken);
		return (created, updated);
	}

	private static async Task<(int Created, int Updated)> UpsertTransactionsAsync(
		ApplicationDbContext context, SqliteConnection sqlite, int exportVersion, CancellationToken cancellationToken)
	{
		if (!TableExists(sqlite, "transactions"))
		{
			return (0, 0);
		}

		// v2+ uses `card_id`; v1 used `account_id`. The TransactionEntity.AccountId column
		// name is preserved on the receipts side — only the imported backup's column differs.
		string cardColumn = exportVersion < 2 ? "account_id" : "card_id";

		int created = 0, updated = 0;
		await using SqliteCommand cmd = sqlite.CreateCommand();
		cmd.CommandText = $"SELECT id, receipt_id, {cardColumn}, amount, amount_currency, date FROM transactions";
		await using SqliteDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

		while (await reader.ReadAsync(cancellationToken))
		{
			Guid id = Guid.Parse(reader.GetString(0));
			Guid receiptId = Guid.Parse(reader.GetString(1));
			Guid accountId = Guid.Parse(reader.GetString(2));
			decimal amount = reader.GetDecimal(3);
			Currency amountCurrency = Enum.Parse<Currency>(reader.GetString(4));
			DateOnly date = DateOnly.Parse(reader.GetString(5));

			TransactionEntity? existing = await context.Transactions
				.IgnoreQueryFilters()
				.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
			if (existing is not null)
			{
				existing.ReceiptId = receiptId;
				existing.AccountId = accountId;
				existing.Amount = amount;
				existing.AmountCurrency = amountCurrency;
				existing.Date = date;
				ClearSoftDelete(existing);
				updated++;
			}
			else
			{
				context.Transactions.Add(new TransactionEntity
				{
					Id = id,
					ReceiptId = receiptId,
					AccountId = accountId,
					Amount = amount,
					AmountCurrency = amountCurrency,
					Date = date,
				});
				created++;
			}
		}

		await context.SaveChangesAsync(cancellationToken);
		return (created, updated);
	}

	private static async Task<(int Created, int Updated)> UpsertAdjustmentsAsync(
		ApplicationDbContext context, SqliteConnection sqlite, CancellationToken cancellationToken)
	{
		if (!TableExists(sqlite, "adjustments"))
		{
			return (0, 0);
		}

		int created = 0, updated = 0;
		await using SqliteCommand cmd = sqlite.CreateCommand();
		cmd.CommandText = "SELECT id, receipt_id, type, amount, amount_currency, description FROM adjustments";
		await using SqliteDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

		while (await reader.ReadAsync(cancellationToken))
		{
			Guid id = Guid.Parse(reader.GetString(0));
			Guid receiptId = Guid.Parse(reader.GetString(1));
			AdjustmentType type = Enum.Parse<AdjustmentType>(reader.GetString(2));
			decimal amount = reader.GetDecimal(3);
			Currency amountCurrency = Enum.Parse<Currency>(reader.GetString(4));
			string? description = reader.IsDBNull(5) ? null : reader.GetString(5);

			AdjustmentEntity? existing = await context.Adjustments
				.IgnoreQueryFilters()
				.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
			if (existing is not null)
			{
				existing.ReceiptId = receiptId;
				existing.Type = type;
				existing.Amount = amount;
				existing.AmountCurrency = amountCurrency;
				existing.Description = description;
				ClearSoftDelete(existing);
				updated++;
			}
			else
			{
				context.Adjustments.Add(new AdjustmentEntity
				{
					Id = id,
					ReceiptId = receiptId,
					Type = type,
					Amount = amount,
					AmountCurrency = amountCurrency,
					Description = description,
				});
				created++;
			}
		}

		await context.SaveChangesAsync(cancellationToken);
		return (created, updated);
	}
}
