using System.Linq.Expressions;
using Application.Models;
using FluentAssertions;
using Infrastructure.Extensions;

namespace Infrastructure.Tests.Extensions;

public class QueryableSortExtensionsTests
{
	private static readonly List<TestEntity> TestData =
	[
		new() { Id = 1, Name = "Charlie", Age = 30 },
		new() { Id = 2, Name = "Alice", Age = 25 },
		new() { Id = 3, Name = "Bob", Age = 35 },
	];

	private static readonly Dictionary<string, Expression<Func<TestEntity, object>>> AllowedColumns = new(StringComparer.OrdinalIgnoreCase)
	{
		["name"] = e => e.Name,
		["age"] = e => e.Age,
		["id"] = e => e.Id,
	};

	private static readonly Expression<Func<TestEntity, object>> DefaultSort = e => e.Name;

	[Fact]
	public void ApplySort_DefaultSortParams_SortsByDefaultColumnAscending()
	{
		// Arrange
		IQueryable<TestEntity> query = TestData.AsQueryable();

		// Act
		List<TestEntity> result = query.ApplySort(SortParams.Default, AllowedColumns, DefaultSort).ToList();

		// Assert
		result.Should().HaveCount(3);
		result[0].Name.Should().Be("Alice");
		result[1].Name.Should().Be("Bob");
		result[2].Name.Should().Be("Charlie");
	}

	[Fact]
	public void ApplySort_ValidColumnAscending_SortsByColumnAscending()
	{
		// Arrange
		IQueryable<TestEntity> query = TestData.AsQueryable();
		SortParams sort = new("age", "asc");

		// Act
		List<TestEntity> result = query.ApplySort(sort, AllowedColumns, DefaultSort).ToList();

		// Assert
		result.Should().HaveCount(3);
		result[0].Age.Should().Be(25);
		result[1].Age.Should().Be(30);
		result[2].Age.Should().Be(35);
	}

	[Fact]
	public void ApplySort_ValidColumnDescending_SortsByColumnDescending()
	{
		// Arrange
		IQueryable<TestEntity> query = TestData.AsQueryable();
		SortParams sort = new("age", "desc");

		// Act
		List<TestEntity> result = query.ApplySort(sort, AllowedColumns, DefaultSort).ToList();

		// Assert
		result.Should().HaveCount(3);
		result[0].Age.Should().Be(35);
		result[1].Age.Should().Be(30);
		result[2].Age.Should().Be(25);
	}

	[Fact]
	public void ApplySort_InvalidColumn_FallsBackToDefaultSort()
	{
		// Arrange
		IQueryable<TestEntity> query = TestData.AsQueryable();
		SortParams sort = new("nonexistent", "asc");

		// Act
		List<TestEntity> result = query.ApplySort(sort, AllowedColumns, DefaultSort).ToList();

		// Assert
		result[0].Name.Should().Be("Alice");
		result[1].Name.Should().Be("Bob");
		result[2].Name.Should().Be("Charlie");
	}

	[Theory]
	[InlineData("name")]
	[InlineData("Name")]
	[InlineData("NAME")]
	[InlineData("nAmE")]
	public void ApplySort_CaseInsensitiveColumnMatch_SortsByMatchedColumn(string columnName)
	{
		// Arrange
		IQueryable<TestEntity> query = TestData.AsQueryable();
		SortParams sort = new(columnName, "desc");

		// Act
		List<TestEntity> result = query.ApplySort(sort, AllowedColumns, DefaultSort).ToList();

		// Assert
		result[0].Name.Should().Be("Charlie");
		result[1].Name.Should().Be("Bob");
		result[2].Name.Should().Be("Alice");
	}

	[Fact]
	public void ApplySort_NullSortByWithExplicitDirection_UsesDefaultSort()
	{
		// Arrange
		IQueryable<TestEntity> query = TestData.AsQueryable();
		SortParams sort = new(null, "desc");

		// Act
		List<TestEntity> result = query.ApplySort(sort, AllowedColumns, DefaultSort).ToList();

		// Assert — default sort is ascending by name regardless of the explicit direction
		result[0].Name.Should().Be("Alice");
		result[1].Name.Should().Be("Bob");
		result[2].Name.Should().Be("Charlie");
	}

	[Fact]
	public void ApplySort_DefaultDescendingTrue_SortsByDefaultColumnDescending()
	{
		// Arrange
		IQueryable<TestEntity> query = TestData.AsQueryable();

		// Act
		List<TestEntity> result = query.ApplySort(SortParams.Default, AllowedColumns, DefaultSort, defaultDescending: true).ToList();

		// Assert
		result[0].Name.Should().Be("Charlie");
		result[1].Name.Should().Be("Bob");
		result[2].Name.Should().Be("Alice");
	}

	[Fact]
	public void ApplySort_ValidColumnOverridesDefaultDescending_UsesExplicitDirection()
	{
		// Arrange
		IQueryable<TestEntity> query = TestData.AsQueryable();
		SortParams sort = new("name", "asc");

		// Act — defaultDescending is true but explicit column sort should use sort.IsDescending (false)
		List<TestEntity> result = query.ApplySort(sort, AllowedColumns, DefaultSort, defaultDescending: true).ToList();

		// Assert
		result[0].Name.Should().Be("Alice");
		result[1].Name.Should().Be("Bob");
		result[2].Name.Should().Be("Charlie");
	}

	[Fact]
	public void ApplySort_EmptySortBy_FallsBackToDefaultSort()
	{
		// Arrange
		IQueryable<TestEntity> query = TestData.AsQueryable();
		SortParams sort = new("", "desc");

		// Act
		List<TestEntity> result = query.ApplySort(sort, AllowedColumns, DefaultSort).ToList();

		// Assert — empty string is treated as no sort column, so default ascending
		result[0].Name.Should().Be("Alice");
		result[1].Name.Should().Be("Bob");
		result[2].Name.Should().Be("Charlie");
	}

	[Fact]
	public void ApplySort_WhitespaceSortBy_FallsBackToDefaultSort()
	{
		// Arrange
		IQueryable<TestEntity> query = TestData.AsQueryable();
		SortParams sort = new("   ", "desc");

		// Act
		List<TestEntity> result = query.ApplySort(sort, AllowedColumns, DefaultSort).ToList();

		// Assert — whitespace-only is treated as no sort column, so default ascending
		result[0].Name.Should().Be("Alice");
		result[1].Name.Should().Be("Bob");
		result[2].Name.Should().Be("Charlie");
	}

	private sealed class TestEntity
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public int Age { get; set; }
	}
}
