using API.Controllers;
using API.Generated.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Presentation.API.Tests.Controllers;

public class MetadataControllerTests
{
	[Fact]
	public void GetEnums_ReturnsOkWithAllCategories()
	{
		MetadataController controller = new();

		Ok<EnumMetadataResponse> result = controller.GetEnums();

		result.Value.Should().NotBeNull();
		result.Value!.AdjustmentTypes.Should().NotBeEmpty();
		result.Value.AuthEventTypes.Should().NotBeEmpty();
		result.Value.PricingModes.Should().NotBeEmpty();
		result.Value.AuditActions.Should().NotBeEmpty();
		result.Value.EntityTypes.Should().NotBeEmpty();
	}

	[Fact]
	public void GetEnums_AllValuesHaveNonEmptyLabels()
	{
		MetadataController controller = new();

		Ok<EnumMetadataResponse> result = controller.GetEnums();

		EnumMetadataResponse response = result.Value!;

		IEnumerable<EnumValuePair> allPairs = response.AdjustmentTypes
			.Concat(response.AuthEventTypes)
			.Concat(response.PricingModes)
			.Concat(response.AuditActions)
			.Concat(response.EntityTypes);

		foreach (EnumValuePair pair in allPairs)
		{
			pair.Value.Should().NotBeNullOrWhiteSpace("every enum entry must have a value");
			pair.Label.Should().NotBeNullOrWhiteSpace("every enum entry must have a label");
		}
	}
}
