using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class CategoryEntityConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
	public void Configure(EntityTypeBuilder<CategoryEntity> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property(e => e.Id)
			.IsRequired()
			.ValueGeneratedOnAdd();

		builder.HasIndex(e => e.Name)
			.IsUnique()
			.HasFilter("\"DeletedAt\" IS NULL");

		builder.HasData(
			new CategoryEntity { Id = new Guid("83a7f4ea-f771-40b3-850f-35b90a3bd05e"), Name = "Groceries", Description = "Food and household supplies" },
			new CategoryEntity { Id = new Guid("e37ce004-56ea-4a33-8983-55a9552d05be"), Name = "Dining", Description = "Restaurants, takeout, and delivery" },
			new CategoryEntity { Id = new Guid("3a131ca1-3300-4cde-b7ee-24704934feea"), Name = "Transportation", Description = "Gas, transit, parking, and rideshare" },
			new CategoryEntity { Id = new Guid("92eae007-7d82-492c-9370-ff64873cc63a"), Name = "Shopping", Description = "Clothing, electronics, and general retail" },
			new CategoryEntity { Id = new Guid("705da6c5-6fb6-4b3c-aef1-f42e5136a499"), Name = "Utilities", Description = "Electric, water, internet, and phone" }
		);

		builder.HasQueryFilter(e => e.DeletedAt == null);
	}
}
