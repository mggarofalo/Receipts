using Application.Models;
using Common;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Tests.Services;

public class BackupImportServiceTests : IDisposable
{
	private readonly string _inMemoryDbName;
	private readonly DbContextOptions<ApplicationDbContext> _options;
	private readonly BackupImportService _service;
	private readonly string _tempDir;

	public BackupImportServiceTests()
	{
		_inMemoryDbName = Guid.NewGuid().ToString();
		_options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: _inMemoryDbName)
			.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
			.Options;

		Moq.Mock<IDbContextFactory<ApplicationDbContext>> factoryMock = new();
		factoryMock
			.Setup(f => f.CreateDbContextAsync(Moq.It.IsAny<CancellationToken>()))
			.Returns(() => Task.FromResult(new ApplicationDbContext(_options)));

		ILogger<BackupImportService> logger = NullLogger<BackupImportService>.Instance;
		_service = new BackupImportService(factoryMock.Object, logger);

		_tempDir = Path.Combine(Path.GetTempPath(), $"backup-import-test-{Guid.NewGuid():N}");
		Directory.CreateDirectory(_tempDir);
	}

	/// <summary>
	/// Creates a fresh context for seeding/asserting that shares the same in-memory database.
	/// </summary>
	private ApplicationDbContext CreateAssertionContext() => new(_options);

	public void Dispose()
	{
		try
		{
			Directory.Delete(_tempDir, recursive: true);
		}
		catch
		{
			// Best effort cleanup
		}
		GC.SuppressFinalize(this);
	}

	[Fact]
	public async Task ImportFromSqliteAsync_InvalidFile_ThrowsInvalidOperationException()
	{
		// Arrange
		using MemoryStream stream = new([0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F]);

		// Act & Assert
		Func<Task> act = () => _service.ImportFromSqliteAsync(stream, CancellationToken.None);
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*not a valid SQLite database*");
	}

	[Fact]
	public async Task ImportFromSqliteAsync_EmptyDatabase_ReturnsZeroCounts()
	{
		// Arrange
		string sqlitePath = CreateEmptySqliteDatabase();
		await using FileStream stream = File.OpenRead(sqlitePath);

		// Act
		BackupImportResult result = await _service.ImportFromSqliteAsync(stream, CancellationToken.None);

		// Assert
		result.TotalCreated.Should().Be(0);
		result.TotalUpdated.Should().Be(0);
	}

	[Fact]
	public async Task ImportFromSqliteAsync_AccountsTable_CreatesNewAccounts()
	{
		// Arrange
		Guid accountId = Guid.NewGuid();
		string sqlitePath = CreateSqliteDatabaseWithAccounts(
			(accountId, "ACC001", "Test Account", true));

		await using FileStream stream = File.OpenRead(sqlitePath);

		// Act
		BackupImportResult result = await _service.ImportFromSqliteAsync(stream, CancellationToken.None);

		// Assert
		result.AccountsCreated.Should().Be(1);
		result.AccountsUpdated.Should().Be(0);

		await using ApplicationDbContext assertCtx = CreateAssertionContext();
		CardEntity? account = await assertCtx.Cards.FindAsync(accountId);
		account.Should().NotBeNull();
		account!.CardCode.Should().Be("ACC001");
		account.Name.Should().Be("Test Account");
		account.IsActive.Should().BeTrue();
	}

	[Fact]
	public async Task ImportFromSqliteAsync_ExistingAccount_UpdatesAccount()
	{
		// Arrange - seed existing account
		Guid accountId = Guid.NewGuid();
		await using (ApplicationDbContext seedCtx = CreateAssertionContext())
		{
			seedCtx.Cards.Add(new CardEntity
			{
				Id = accountId,
				CardCode = "OLD001",
				Name = "Old Name",
				IsActive = false,
			});
			await seedCtx.SaveChangesAsync();
		}

		// Create SQLite with updated values
		string sqlitePath = CreateSqliteDatabaseWithAccounts(
			(accountId, "NEW001", "New Name", true));

		await using FileStream stream = File.OpenRead(sqlitePath);

		// Act
		BackupImportResult result = await _service.ImportFromSqliteAsync(stream, CancellationToken.None);

		// Assert
		result.AccountsCreated.Should().Be(0);
		result.AccountsUpdated.Should().Be(1);

		await using ApplicationDbContext assertCtx = CreateAssertionContext();
		CardEntity? account = await assertCtx.Cards.FindAsync(accountId);
		account.Should().NotBeNull();
		account!.CardCode.Should().Be("NEW001");
		account.Name.Should().Be("New Name");
		account.IsActive.Should().BeTrue();
	}

	[Fact]
	public async Task ImportFromSqliteAsync_CategoriesTable_CreatesNewCategories()
	{
		// Arrange
		Guid categoryId = Guid.NewGuid();
		string sqlitePath = CreateSqliteDatabaseWithCategories(
			(categoryId, "Groceries", "Food and household items", true));

		await using FileStream stream = File.OpenRead(sqlitePath);

		// Act
		BackupImportResult result = await _service.ImportFromSqliteAsync(stream, CancellationToken.None);

		// Assert
		result.CategoriesCreated.Should().Be(1);
		result.CategoriesUpdated.Should().Be(0);
	}

	[Fact]
	public async Task ImportFromSqliteAsync_ReceiptsTable_CreatesNewReceipts()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		string sqlitePath = CreateSqliteDatabaseWithReceipts(
			(receiptId, "Walmart", "2024-06-15", 2.50m, "USD"));

		await using FileStream stream = File.OpenRead(sqlitePath);

		// Act
		BackupImportResult result = await _service.ImportFromSqliteAsync(stream, CancellationToken.None);

		// Assert
		result.ReceiptsCreated.Should().Be(1);
		result.ReceiptsUpdated.Should().Be(0);

		await using ApplicationDbContext assertCtx = CreateAssertionContext();
		ReceiptEntity? receipt = await assertCtx.Receipts.FindAsync(receiptId);
		receipt.Should().NotBeNull();
		receipt!.Location.Should().Be("Walmart");
		receipt.Date.Should().Be(new DateOnly(2024, 6, 15));
		receipt.TaxAmount.Should().Be(2.50m);
		receipt.TaxAmountCurrency.Should().Be(Currency.USD);
	}

	[Fact]
	public async Task ImportFromSqliteAsync_MixedCreateAndUpdate_ReturnsCorrectCounts()
	{
		// Arrange - seed an existing account
		Guid existingId = Guid.NewGuid();
		await using (ApplicationDbContext seedCtx = CreateAssertionContext())
		{
			seedCtx.Cards.Add(new CardEntity
			{
				Id = existingId,
				CardCode = "EXIST",
				Name = "Existing",
				IsActive = true,
			});
			await seedCtx.SaveChangesAsync();
		}

		Guid newId = Guid.NewGuid();
		string sqlitePath = CreateSqliteDatabaseWithAccounts(
			(existingId, "UPDATED", "Updated Name", false),
			(newId, "NEW001", "Brand New", true));

		await using FileStream stream = File.OpenRead(sqlitePath);

		// Act
		BackupImportResult result = await _service.ImportFromSqliteAsync(stream, CancellationToken.None);

		// Assert
		result.AccountsCreated.Should().Be(1);
		result.AccountsUpdated.Should().Be(1);
	}

	[Fact]
	public async Task ImportFromSqliteAsync_MissingTables_SkipsGracefully()
	{
		// Arrange - SQLite with only accounts table, no other tables
		string sqlitePath = CreateSqliteDatabaseWithAccounts(
			(Guid.NewGuid(), "ACC001", "Test", true));

		await using FileStream stream = File.OpenRead(sqlitePath);

		// Act
		BackupImportResult result = await _service.ImportFromSqliteAsync(stream, CancellationToken.None);

		// Assert
		result.AccountsCreated.Should().Be(1);
		result.CategoriesCreated.Should().Be(0);
		result.ReceiptsCreated.Should().Be(0);
		result.ReceiptItemsCreated.Should().Be(0);
		result.TransactionsCreated.Should().Be(0);
		result.AdjustmentsCreated.Should().Be(0);
	}

	#region SQLite Test Helpers

	private string CreateEmptySqliteDatabase()
	{
		string path = Path.Combine(_tempDir, $"{Guid.NewGuid():N}.sqlite");
		using SqliteConnection conn = new($"Data Source={path}");
		conn.Open();
		// Force SQLite to write the file header by creating and dropping a dummy table
		using SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = "CREATE TABLE _dummy (id INTEGER); DROP TABLE _dummy;";
		cmd.ExecuteNonQuery();
		return path;
	}

	private string CreateSqliteDatabaseWithAccounts(params (Guid Id, string CardCode, string Name, bool IsActive)[] accounts)
	{
		string path = Path.Combine(_tempDir, $"{Guid.NewGuid():N}.sqlite");
		using SqliteConnection conn = new($"Data Source={path}");
		conn.Open();

		using SqliteCommand createCmd = conn.CreateCommand();
		createCmd.CommandText = @"
			CREATE TABLE accounts (
				id TEXT NOT NULL PRIMARY KEY,
				account_code TEXT NOT NULL,
				name TEXT NOT NULL,
				is_active INTEGER NOT NULL
			)";
		createCmd.ExecuteNonQuery();

		foreach ((Guid id, string accountCode, string name, bool isActive) in accounts)
		{
			using SqliteCommand insertCmd = conn.CreateCommand();
			insertCmd.CommandText = "INSERT INTO accounts (id, account_code, name, is_active) VALUES (@id, @code, @name, @active)";
			insertCmd.Parameters.AddWithValue("@id", id.ToString());
			insertCmd.Parameters.AddWithValue("@code", accountCode);
			insertCmd.Parameters.AddWithValue("@name", name);
			insertCmd.Parameters.AddWithValue("@active", isActive);
			insertCmd.ExecuteNonQuery();
		}

		return path;
	}

	private string CreateSqliteDatabaseWithCategories(params (Guid Id, string Name, string? Description, bool IsActive)[] categories)
	{
		string path = Path.Combine(_tempDir, $"{Guid.NewGuid():N}.sqlite");
		using SqliteConnection conn = new($"Data Source={path}");
		conn.Open();

		using SqliteCommand createCmd = conn.CreateCommand();
		createCmd.CommandText = @"
			CREATE TABLE categories (
				id TEXT NOT NULL PRIMARY KEY,
				name TEXT NOT NULL,
				description TEXT,
				is_active INTEGER NOT NULL
			)";
		createCmd.ExecuteNonQuery();

		foreach ((Guid id, string name, string? description, bool isActive) in categories)
		{
			using SqliteCommand insertCmd = conn.CreateCommand();
			insertCmd.CommandText = "INSERT INTO categories (id, name, description, is_active) VALUES (@id, @name, @desc, @active)";
			insertCmd.Parameters.AddWithValue("@id", id.ToString());
			insertCmd.Parameters.AddWithValue("@name", name);
			insertCmd.Parameters.AddWithValue("@desc", (object?)description ?? DBNull.Value);
			insertCmd.Parameters.AddWithValue("@active", isActive);
			insertCmd.ExecuteNonQuery();
		}

		return path;
	}

	private string CreateSqliteDatabaseWithReceipts(params (Guid Id, string Location, string Date, decimal TaxAmount, string TaxAmountCurrency)[] receipts)
	{
		string path = Path.Combine(_tempDir, $"{Guid.NewGuid():N}.sqlite");
		using SqliteConnection conn = new($"Data Source={path}");
		conn.Open();

		using SqliteCommand createCmd = conn.CreateCommand();
		createCmd.CommandText = @"
			CREATE TABLE receipts (
				id TEXT NOT NULL PRIMARY KEY,
				location TEXT NOT NULL,
				date TEXT NOT NULL,
				tax_amount REAL NOT NULL,
				tax_amount_currency TEXT NOT NULL
			)";
		createCmd.ExecuteNonQuery();

		foreach ((Guid id, string location, string date, decimal taxAmount, string taxAmountCurrency) in receipts)
		{
			using SqliteCommand insertCmd = conn.CreateCommand();
			insertCmd.CommandText = "INSERT INTO receipts (id, location, date, tax_amount, tax_amount_currency) VALUES (@id, @loc, @date, @tax, @currency)";
			insertCmd.Parameters.AddWithValue("@id", id.ToString());
			insertCmd.Parameters.AddWithValue("@loc", location);
			insertCmd.Parameters.AddWithValue("@date", date);
			insertCmd.Parameters.AddWithValue("@tax", (double)taxAmount);
			insertCmd.Parameters.AddWithValue("@currency", taxAmountCurrency);
			insertCmd.ExecuteNonQuery();
		}

		return path;
	}

	#endregion
}
