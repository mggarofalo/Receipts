using System.Text.Json;
using API.Configuration;
using Domain.NormalizedDescriptions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Presentation.API.Tests.Configuration;

/// <summary>
/// Regression test for RECEIPTS-534: verifies that the API's configured
/// JsonSerializerOptions serialize enum values as camelCase. Prior to the fix,
/// <see cref="ApplicationConfiguration"/> registered <c>JsonStringEnumConverter</c>
/// without a naming policy, producing PascalCase output that drifted from the
/// OpenAPI spec.
/// </summary>
public class JsonSerializerEnumCasingTests
{
	private static JsonSerializerOptions GetConfiguredJsonOptions()
	{
		ServiceCollection services = new();
		services.AddApplicationServices(new ConfigurationBuilder().Build());
		ServiceProvider provider = services.BuildServiceProvider();
		JsonOptions mvcJsonOptions = provider.GetRequiredService<IOptions<JsonOptions>>().Value;
		return mvcJsonOptions.JsonSerializerOptions;
	}

	[Fact]
	public void JsonStringEnumConverter_SerializesEnumValuesAsCamelCase()
	{
		JsonSerializerOptions options = GetConfiguredJsonOptions();

		string activeJson = JsonSerializer.Serialize(NormalizedDescriptionStatus.Active, options);
		string pendingJson = JsonSerializer.Serialize(NormalizedDescriptionStatus.PendingReview, options);

		activeJson.Should().Be("\"active\"");
		pendingJson.Should().Be("\"pendingReview\"");
	}

	[Fact]
	public void JsonStringEnumConverter_DeserializesCamelCaseEnumValues()
	{
		JsonSerializerOptions options = GetConfiguredJsonOptions();

		NormalizedDescriptionStatus active = JsonSerializer.Deserialize<NormalizedDescriptionStatus>("\"active\"", options);
		NormalizedDescriptionStatus pending = JsonSerializer.Deserialize<NormalizedDescriptionStatus>("\"pendingReview\"", options);

		active.Should().Be(NormalizedDescriptionStatus.Active);
		pending.Should().Be(NormalizedDescriptionStatus.PendingReview);
	}
}
