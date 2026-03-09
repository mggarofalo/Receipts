using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ItemEmbeddingEntityConfiguration : IEntityTypeConfiguration<ItemEmbeddingEntity>
{
	public void Configure(EntityTypeBuilder<ItemEmbeddingEntity> builder)
	{
		builder.ToTable("ItemEmbeddings");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Id)
			.IsRequired()
			.ValueGeneratedOnAdd();

		builder.Property(e => e.Embedding)
			.HasColumnType("vector(384)");

		builder.HasIndex(e => new { e.EntityType, e.EntityId })
			.IsUnique();
	}
}
