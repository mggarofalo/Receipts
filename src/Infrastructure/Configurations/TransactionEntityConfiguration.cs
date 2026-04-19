using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TransactionEntityConfiguration : IEntityTypeConfiguration<TransactionEntity>
{
	public void Configure(EntityTypeBuilder<TransactionEntity> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property(e => e.Id)
			.IsRequired()
			.ValueGeneratedOnAdd();

		builder.Navigation(e => e.Receipt)
			.AutoInclude();

		builder.HasOne(e => e.Account)
			.WithMany()
			.HasForeignKey(e => e.AccountId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Navigation(e => e.Account)
			.AutoInclude();

		// RECEIPTS-553: Transaction→Card link, restoring card-of-origin destroyed by
		// AccountMergeService when the Account aggregate was introduced. CardId is
		// nullable for now (backfilled from the 2026-04-18 pre-deploy backup);
		// promoting to NOT NULL and removing AccountId is a follow-up.
		builder.HasOne(e => e.Card)
			.WithMany()
			.HasForeignKey(e => e.CardId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.Navigation(e => e.Card)
			.AutoInclude();

		builder.HasQueryFilter(e => e.DeletedAt == null);
	}
}
