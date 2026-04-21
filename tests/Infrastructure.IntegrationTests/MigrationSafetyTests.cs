using FluentAssertions;
using Infrastructure.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Infrastructure.IntegrationTests;

[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class MigrationSafetyTests(PostgresFixture fixture)
{
	// RECEIPTS-574: the PromoteTransactionCardIdNotNull migration includes a pre-check
	// guard that refuses to apply if any Transactions.CardId IS NULL rows remain.
	// This test exercises that guard by rolling the DB back to the prior (nullable)
	// state, inserting a row with NULL CardId, then attempting to reapply — and
	// asserts that the migration throws with the specific guard error message.
	[Fact]
	public async Task PromoteTransactionCardIdNotNull_WithNullCardIdRow_AbortsWithGuardError()
	{
		const string priorMigration = "20260419022200_AddCardIdToTransactions";

		await using ApplicationDbContext context = fixture.CreateDbContext();
		IMigrator migrator = context.GetInfrastructure().GetRequiredService<IMigrator>();

		// Roll back to the pre-574 state where CardId is nullable.
		await migrator.MigrateAsync(priorMigration);

		// Insert a minimal Transaction row with NULL CardId. Use raw SQL to bypass EF's
		// non-null CardId constraint on the current entity model.
		Guid txId = Guid.NewGuid();
		Guid receiptId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		await context.Database.ExecuteSqlRawAsync(
			"""
			INSERT INTO "Accounts" ("Id", "Name", "IsActive") VALUES ({0}, 'Guard Test', true);
			INSERT INTO "Receipts" ("Id", "Location", "Date", "TaxAmount", "TaxAmountCurrency")
				VALUES ({1}, 'Guard Test', CURRENT_DATE, 0, 'USD');
			INSERT INTO "Transactions" ("Id", "ReceiptId", "AccountId", "CardId", "Amount", "AmountCurrency", "Date")
				VALUES ({2}, {1}, {0}, NULL, 1, 'USD', CURRENT_DATE);
			""",
			accountId, receiptId, txId);

		// Attempt to reapply all pending migrations (which includes 574). The guard
		// should raise an exception; the Npgsql provider surfaces it as PostgresException.
		Func<Task> act = () => migrator.MigrateAsync();

		PostgresException ex = (await act.Should().ThrowAsync<PostgresException>())
			.Where(e => e.MessageText.Contains("RECEIPTS-574"))
			.Subject.First();
		ex.MessageText.Should().Contain("cannot promote Transactions.CardId to NOT NULL");

		// The row should still exist in its original nullable state.
		long nullCardIdCount = await context.Transactions
			.IgnoreQueryFilters()
			.LongCountAsync(t => t.Id == txId);
		nullCardIdCount.Should().Be(1);

		// Clean up: delete the offending row so later tests (which share the fixture)
		// are not blocked, then reapply the migration to leave the fixture in its
		// canonical post-migration state.
		await context.Database.ExecuteSqlRawAsync("""DELETE FROM "Transactions" WHERE "Id" = {0};""", txId);
		await context.Database.ExecuteSqlRawAsync("""DELETE FROM "Receipts" WHERE "Id" = {0};""", receiptId);
		await context.Database.ExecuteSqlRawAsync("""DELETE FROM "Accounts" WHERE "Id" = {0};""", accountId);
		await migrator.MigrateAsync();
	}
}
