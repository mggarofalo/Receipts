using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class CardEntityConfiguration : IEntityTypeConfiguration<CardEntity>
{
	public void Configure(EntityTypeBuilder<CardEntity> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property(e => e.Id)
			.IsRequired()
			.ValueGeneratedOnAdd();
	}
}
