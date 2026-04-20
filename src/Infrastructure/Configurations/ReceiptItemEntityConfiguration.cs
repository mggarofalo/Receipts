using Common;
using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ReceiptItemEntityConfiguration : IEntityTypeConfiguration<ReceiptItemEntity>
{
	public void Configure(EntityTypeBuilder<ReceiptItemEntity> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property(e => e.Id)
			.IsRequired()
			.ValueGeneratedOnAdd();

		builder.Property(e => e.PricingMode)
			.HasConversion(
				v => v.ToString().ToLowerInvariant(),
				v => Enum.Parse<PricingMode>(v, ignoreCase: true))
			.HasMaxLength(8);

		builder.Navigation(e => e.Receipt)
			.AutoInclude();

		builder.HasOne(e => e.NormalizedDescription)
			.WithMany()
			.HasForeignKey(e => e.NormalizedDescriptionId)
			.OnDelete(DeleteBehavior.SetNull);

		builder.Navigation(e => e.NormalizedDescription)
			.AutoInclude();

		builder.HasIndex(e => e.NormalizedDescriptionId);

		builder.HasQueryFilter(e => e.DeletedAt == null);
	}
}
