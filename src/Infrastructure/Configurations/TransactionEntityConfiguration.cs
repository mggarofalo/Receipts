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

		builder.Navigation(e => e.Account)
			.AutoInclude();

		builder.HasQueryFilter(e => e.DeletedAt == null);
	}
}
