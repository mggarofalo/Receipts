using Common;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Tests.Services;

/// <summary>
/// End-to-end startup-validation tests for the options classes registered by
/// <see cref="InfrastructureService.RegisterInfrastructureServices"/> and
/// <see cref="InfrastructureService.AddVlmOcrClient(IServiceCollection, IConfiguration)"/>
/// (RECEIPTS-638). These tests build a real <see cref="ServiceProvider"/> from the
/// production registration code path and verify that DataAnnotations validation surfaces
/// at first <see cref="IOptions{TOptions}"/> resolution rather than at the first user
/// request.
/// </summary>
public class InfrastructureServiceOptionsTests
{
	[Fact]
	public void AddVlmOcrClient_BadTimeoutSeconds_FailsAtStartup()
	{
		// Arrange — TimeoutSeconds outside [Range(1, 3600)] is the canonical "misconfig fails
		// at startup" assertion called out by RECEIPTS-638. Without ValidateOnStart this would
		// surface as a confusing TimeoutRejectedException on the first scan request.
		ServiceCollection services = new();
		services.AddLogging();
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				[$"{ConfigurationVariables.OcrVlmSection}:{nameof(VlmOcrOptions.OllamaUrl)}"] = "http://test-ollama",
				[$"{ConfigurationVariables.OcrVlmSection}:{nameof(VlmOcrOptions.Model)}"] = "glm-ocr:q8_0",
				[$"{ConfigurationVariables.OcrVlmSection}:{nameof(VlmOcrOptions.TimeoutSeconds)}"] = "9999",
			})
			.Build();
		services.AddVlmOcrClient(configuration);

		// Act
		using ServiceProvider sp = services.BuildServiceProvider();
		Func<VlmOcrOptions> act = () => sp.GetRequiredService<IOptions<VlmOcrOptions>>().Value;

		// Assert
		act.Should().Throw<OptionsValidationException>()
			.WithMessage("*TimeoutSeconds*");
	}

	[Fact]
	public void AddVlmOcrClient_EmptyModel_FailsAtStartup()
	{
		// Arrange — empty Model violates [Required(AllowEmptyStrings = false)]. Smoke test
		// (VlmOcrSmokeTest) and OllamaReceiptExtractionService both depend on a non-empty
		// model name; validating at startup keeps the failure mode obvious.
		ServiceCollection services = new();
		services.AddLogging();
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				[$"{ConfigurationVariables.OcrVlmSection}:{nameof(VlmOcrOptions.OllamaUrl)}"] = "http://test-ollama",
				[$"{ConfigurationVariables.OcrVlmSection}:{nameof(VlmOcrOptions.Model)}"] = string.Empty,
			})
			.Build();
		services.AddVlmOcrClient(configuration);

		// Act
		using ServiceProvider sp = services.BuildServiceProvider();
		Func<VlmOcrOptions> act = () => sp.GetRequiredService<IOptions<VlmOcrOptions>>().Value;

		// Assert
		act.Should().Throw<OptionsValidationException>()
			.WithMessage("*Model*");
	}

	[Fact]
	public void AddVlmOcrClient_HappyPath_ResolvesValidOptions()
	{
		// Arrange — sanity check that valid configuration resolves without throwing and that
		// every property round-trips through the binder.
		ServiceCollection services = new();
		services.AddLogging();
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				[$"{ConfigurationVariables.OcrVlmSection}:{nameof(VlmOcrOptions.OllamaUrl)}"] = "http://test-ollama:11434",
				[$"{ConfigurationVariables.OcrVlmSection}:{nameof(VlmOcrOptions.Model)}"] = "qwen2.5vl:7b",
				[$"{ConfigurationVariables.OcrVlmSection}:{nameof(VlmOcrOptions.TimeoutSeconds)}"] = "60",
				[$"{ConfigurationVariables.OcrVlmSection}:{nameof(VlmOcrOptions.LogRawResponses)}"] = "true",
			})
			.Build();
		services.AddVlmOcrClient(configuration);

		// Act
		using ServiceProvider sp = services.BuildServiceProvider();
		VlmOcrOptions value = sp.GetRequiredService<IOptions<VlmOcrOptions>>().Value;

		// Assert
		value.OllamaUrl.Should().Be("http://test-ollama:11434");
		value.Model.Should().Be("qwen2.5vl:7b");
		value.TimeoutSeconds.Should().Be(60);
		value.LogRawResponses.Should().BeTrue();
	}
}
