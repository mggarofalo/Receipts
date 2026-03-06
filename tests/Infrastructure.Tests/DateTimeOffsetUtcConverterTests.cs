using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Tests;

public class DateTimeOffsetUtcConverterTests
{
	[Fact]
	public void NpgsqlModel_DateTimeOffsetProperties_HaveUtcConverter()
	{
		// Arrange — use the DesignTimeDbContextFactory which configures Npgsql
		DesignTimeDbContextFactory factory = new();
		Environment.SetEnvironmentVariable("POSTGRES_CONNECTION_STRING",
			"Host=localhost;Database=testdb;Username=testuser;Password=testpass");

		using ApplicationDbContext context = factory.CreateDbContext([]);
		IModel model = context.Model;

		// Act + Assert — every DateTimeOffset property should have a value converter
		foreach (IEntityType entityType in model.GetEntityTypes())
		{
			foreach (IProperty property in entityType.GetProperties())
			{
				Type baseType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
				if (baseType != typeof(DateTimeOffset))
				{
					continue;
				}

				ValueConverter? converter = property.GetValueConverter();
				Assert.True(converter is not null,
					$"{entityType.ClrType.Name}.{property.Name} (DateTimeOffset) is missing a UTC value converter");
			}
		}
	}

	[Fact]
	public void UtcConverter_ConvertsNonUtcOffset_ToUtc()
	{
		// Arrange — use the DesignTimeDbContextFactory which configures Npgsql
		DesignTimeDbContextFactory factory = new();
		Environment.SetEnvironmentVariable("POSTGRES_CONNECTION_STRING",
			"Host=localhost;Database=testdb;Username=testuser;Password=testpass");

		using ApplicationDbContext context = factory.CreateDbContext([]);

		// Find a DateTimeOffset property and get its converter
		IProperty dtoProperty = context.Model
			.GetEntityTypes()
			.SelectMany(e => e.GetProperties())
			.First(p => (Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType) == typeof(DateTimeOffset));

		ValueConverter converter = dtoProperty.GetValueConverter()!;

		// Act — convert a DateTimeOffset with -05:00 offset
		DateTimeOffset eastern = new(2026, 3, 6, 12, 0, 0, TimeSpan.FromHours(-5));
		object? result = converter.ConvertToProvider(eastern);

		// Assert — should be normalized to UTC (offset = 0, time adjusted)
		DateTimeOffset utcResult = Assert.IsType<DateTimeOffset>(result);
		Assert.Equal(TimeSpan.Zero, utcResult.Offset);
		Assert.Equal(new DateTimeOffset(2026, 3, 6, 17, 0, 0, TimeSpan.Zero), utcResult);
	}
}
