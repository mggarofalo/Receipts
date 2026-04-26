using System.Text.Json;
using API.Configuration;
using Application.Models.Ocr;
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

		string highJson = JsonSerializer.Serialize(ConfidenceLevel.High, options);
		string mediumJson = JsonSerializer.Serialize(ConfidenceLevel.Medium, options);
		string lowJson = JsonSerializer.Serialize(ConfidenceLevel.Low, options);
		string noneJson = JsonSerializer.Serialize(ConfidenceLevel.None, options);

		highJson.Should().Be("\"high\"");
		mediumJson.Should().Be("\"medium\"");
		lowJson.Should().Be("\"low\"");
		noneJson.Should().Be("\"none\"");
	}

	[Fact]
	public void JsonStringEnumConverter_DeserializesCamelCaseEnumValues()
	{
		JsonSerializerOptions options = GetConfiguredJsonOptions();

		ConfidenceLevel high = JsonSerializer.Deserialize<ConfidenceLevel>("\"high\"", options);
		ConfidenceLevel medium = JsonSerializer.Deserialize<ConfidenceLevel>("\"medium\"", options);
		ConfidenceLevel low = JsonSerializer.Deserialize<ConfidenceLevel>("\"low\"", options);
		ConfidenceLevel none = JsonSerializer.Deserialize<ConfidenceLevel>("\"none\"", options);

		high.Should().Be(ConfidenceLevel.High);
		medium.Should().Be(ConfidenceLevel.Medium);
		low.Should().Be(ConfidenceLevel.Low);
		none.Should().Be(ConfidenceLevel.None);
	}
}
