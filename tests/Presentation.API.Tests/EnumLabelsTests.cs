using API;
using API.Generated.Dtos;
using Common;
using FluentAssertions;
using Infrastructure.Entities.Audit;

namespace Presentation.API.Tests;

public class EnumLabelsTests
{
	[Fact]
	public void AdjustmentTypes_CoversAllEnumValues()
	{
		string[] expected = Enum.GetNames<AdjustmentType>();
		string[] actual = EnumLabels.AdjustmentTypes.Select(p => p.Value).ToArray();

		actual.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public void AuthEventTypes_CoversAllEnumValues()
	{
		string[] expected = Enum.GetNames<AuthEventType>();
		string[] actual = EnumLabels.AuthEventTypes.Select(p => p.Value).ToArray();

		actual.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public void PricingModes_CoversAllEnumValues()
	{
		string[] expected = Enum.GetNames<PricingMode>()
			.Select(n => n.ToLowerInvariant())
			.ToArray();
		string[] actual = EnumLabels.PricingModes.Select(p => p.Value).ToArray();

		actual.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public void AuditActions_CoversAllEnumValues()
	{
		string[] expected = Enum.GetNames<AuditAction>();
		string[] actual = EnumLabels.AuditActions.Select(p => p.Value).ToArray();

		actual.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public void NoDuplicateValues()
	{
		AssertNoDuplicates(EnumLabels.AdjustmentTypes, "AdjustmentTypes");
		AssertNoDuplicates(EnumLabels.AuthEventTypes, "AuthEventTypes");
		AssertNoDuplicates(EnumLabels.PricingModes, "PricingModes");
		AssertNoDuplicates(EnumLabels.AuditActions, "AuditActions");
		AssertNoDuplicates(EnumLabels.EntityTypes, "EntityTypes");
	}

	private static void AssertNoDuplicates(
		IEnumerable<EnumValuePair> pairs,
		string listName)
	{
		string[] values = pairs.Select(p => p.Value).ToArray();
		values.Should().OnlyHaveUniqueItems($"{listName} should not contain duplicate values");
	}
}
