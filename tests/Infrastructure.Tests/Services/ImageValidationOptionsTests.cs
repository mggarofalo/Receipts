using System.ComponentModel.DataAnnotations;
using Common;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Tests.Services;

/// <summary>
/// Validation tests for <see cref="ImageValidationOptions"/>. The options class is the new
/// home for the image-pixel-dimension thresholds that previously lived as
/// <c>private const int MaxPixelWidth/Height = 10_000</c> on
/// <see cref="ImageValidationService"/> (RECEIPTS-638). Misconfiguring the options must
/// fail loudly at startup via <see cref="OptionsBuilderExtensions.ValidateOnStart{TOptions}"/>.
/// </summary>
public class ImageValidationOptionsTests
{
	[Fact]
	public void Defaults_MatchHistoricalConstants()
	{
		// The defaults must continue to match the historical hardcoded values so behaviour
		// is unchanged for callers that don't opt into the new section.
		ImageValidationOptions options = new();

		options.MaxPixelWidth.Should().Be(10_000);
		options.MaxPixelHeight.Should().Be(10_000);
	}

	[Fact]
	public void DataAnnotations_RejectZeroOrNegativeMaxPixelWidth()
	{
		// Arrange
		ImageValidationOptions options = new() { MaxPixelWidth = 0, MaxPixelHeight = 10_000 };
		List<ValidationResult> results = [];

		// Act
		bool valid = Validator.TryValidateObject(
			options,
			new ValidationContext(options),
			results,
			validateAllProperties: true);

		// Assert
		valid.Should().BeFalse();
		results.Should().Contain(r => r.MemberNames.Contains(nameof(ImageValidationOptions.MaxPixelWidth)));
	}

	[Fact]
	public void DataAnnotations_RejectZeroOrNegativeMaxPixelHeight()
	{
		// Arrange
		ImageValidationOptions options = new() { MaxPixelWidth = 10_000, MaxPixelHeight = -10 };
		List<ValidationResult> results = [];

		// Act
		bool valid = Validator.TryValidateObject(
			options,
			new ValidationContext(options),
			results,
			validateAllProperties: true);

		// Assert
		valid.Should().BeFalse();
		results.Should().Contain(r => r.MemberNames.Contains(nameof(ImageValidationOptions.MaxPixelHeight)));
	}

	[Fact]
	public void ValidateOnStart_BadConfiguration_ThrowsWhenResolved()
	{
		// Arrange — RECEIPTS-638 acceptance: misconfigured options fail at startup.
		ServiceCollection services = new();
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				[$"{ConfigurationVariables.ImageValidationSection}:{nameof(ImageValidationOptions.MaxPixelWidth)}"] = "0",
			})
			.Build();

		services.AddOptions<ImageValidationOptions>()
			.Bind(configuration.GetSection(ConfigurationVariables.ImageValidationSection))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		// Act
		using ServiceProvider sp = services.BuildServiceProvider();
		Func<ImageValidationOptions> act = () => sp.GetRequiredService<IOptions<ImageValidationOptions>>().Value;

		// Assert
		act.Should().Throw<OptionsValidationException>()
			.WithMessage("*MaxPixelWidth*");
	}

	[Fact]
	public void ValidateOnStart_ValidConfiguration_ResolvesOk()
	{
		// Arrange
		ServiceCollection services = new();
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				[$"{ConfigurationVariables.ImageValidationSection}:{nameof(ImageValidationOptions.MaxPixelWidth)}"] = "5000",
				[$"{ConfigurationVariables.ImageValidationSection}:{nameof(ImageValidationOptions.MaxPixelHeight)}"] = "5000",
			})
			.Build();

		services.AddOptions<ImageValidationOptions>()
			.Bind(configuration.GetSection(ConfigurationVariables.ImageValidationSection))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		// Act
		using ServiceProvider sp = services.BuildServiceProvider();
		ImageValidationOptions value = sp.GetRequiredService<IOptions<ImageValidationOptions>>().Value;

		// Assert
		value.MaxPixelWidth.Should().Be(5000);
		value.MaxPixelHeight.Should().Be(5000);
	}
}
