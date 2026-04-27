using System.ComponentModel.DataAnnotations;
using Common;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Tests.Services;

/// <summary>
/// Validation tests for <see cref="PdfConversionOptions"/>. The options class is the new home
/// for the PDF page-count threshold that previously lived as <c>const int MaxPages = 50</c>
/// on <c>IPdfConversionService</c> (RECEIPTS-638). Misconfiguring the options must fail
/// loudly at startup via <see cref="OptionsBuilderExtensions.ValidateOnStart{TOptions}"/>
/// rather than at the first user upload.
/// </summary>
public class PdfConversionOptionsTests
{
	[Fact]
	public void Defaults_MatchHistoricalConstant()
	{
		// The default MaxPages must continue to match the historical hardcoded value
		// (previously `const int MaxPages = 50` on the interface) so behaviour is unchanged
		// for callers that don't opt into the new section.
		PdfConversionOptions options = new();

		options.MaxPages.Should().Be(50);
	}

	[Fact]
	public void DataAnnotations_RejectMaxPagesZero()
	{
		// Arrange
		PdfConversionOptions options = new() { MaxPages = 0 };
		List<ValidationResult> results = [];

		// Act
		bool valid = Validator.TryValidateObject(
			options,
			new ValidationContext(options),
			results,
			validateAllProperties: true);

		// Assert
		valid.Should().BeFalse();
		results.Should().Contain(r => r.MemberNames.Contains(nameof(PdfConversionOptions.MaxPages)));
	}

	[Fact]
	public void DataAnnotations_RejectNegativeMaxPages()
	{
		// Arrange
		PdfConversionOptions options = new() { MaxPages = -1 };
		List<ValidationResult> results = [];

		// Act
		bool valid = Validator.TryValidateObject(
			options,
			new ValidationContext(options),
			results,
			validateAllProperties: true);

		// Assert
		valid.Should().BeFalse();
	}

	[Fact]
	public void ValidateOnStart_BadConfiguration_ThrowsWhenResolved()
	{
		// Arrange — the canonical "misconfigured options fail at startup" assertion that
		// RECEIPTS-638 calls out as an acceptance criterion. ValidateOnStart causes
		// DataAnnotations errors to surface when IOptions<T> is first resolved (or when
		// the host starts via IHostedService).
		ServiceCollection services = new();
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				[$"{ConfigurationVariables.PdfConversionSection}:{nameof(PdfConversionOptions.MaxPages)}"] = "0",
			})
			.Build();

		services.AddOptions<PdfConversionOptions>()
			.Bind(configuration.GetSection(ConfigurationVariables.PdfConversionSection))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		// Act
		using ServiceProvider sp = services.BuildServiceProvider();
		Func<PdfConversionOptions> act = () => sp.GetRequiredService<IOptions<PdfConversionOptions>>().Value;

		// Assert
		act.Should().Throw<OptionsValidationException>()
			.WithMessage("*MaxPages*");
	}

	[Fact]
	public void ValidateOnStart_ValidConfiguration_ResolvesOk()
	{
		// Arrange — happy path: a valid override resolves without throwing and reports the
		// configured value. Guards against a bad refactor accidentally rejecting all
		// configurations (e.g. inverted Range bounds).
		ServiceCollection services = new();
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				[$"{ConfigurationVariables.PdfConversionSection}:{nameof(PdfConversionOptions.MaxPages)}"] = "100",
			})
			.Build();

		services.AddOptions<PdfConversionOptions>()
			.Bind(configuration.GetSection(ConfigurationVariables.PdfConversionSection))
			.ValidateDataAnnotations()
			.ValidateOnStart();

		// Act
		using ServiceProvider sp = services.BuildServiceProvider();
		PdfConversionOptions value = sp.GetRequiredService<IOptions<PdfConversionOptions>>().Value;

		// Assert
		value.MaxPages.Should().Be(100);
	}
}
