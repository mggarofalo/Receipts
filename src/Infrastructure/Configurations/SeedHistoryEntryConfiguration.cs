using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class SeedHistoryEntryConfiguration : IEntityTypeConfiguration<SeedHistoryEntry>
{
	public void Configure(EntityTypeBuilder<SeedHistoryEntry> builder)
	{
		builder.ToTable("__SeedHistory");
		builder.HasKey(e => e.SeedId);

		builder.Property(e => e.SeedId)
			.HasMaxLength(150)
			.IsRequired();
	}
}
