using Application.Interfaces.Services;
using Common;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Tests.Services;

public class PaddleOcrEngineTests
{
	#region DI Registration

	[Fact]
	public void RegisterOcrEngine_DefaultConfig_RegistersTesseract()
	{
		// Arrange
		ServiceCollection services = new();
		services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
		services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>())
			.Build();

		// Act
		InfrastructureService.RegisterOcrEngine(services, configuration);

		// Assert — descriptor should be for TesseractOcrEngine
		ServiceDescriptor? descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IOcrEngine));
		descriptor.Should().NotBeNull();
		descriptor!.ImplementationType.Should().Be(typeof(TesseractOcrEngine));
		descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
	}

	[Fact]
	public void RegisterOcrEngine_TesseractExplicit_RegistersTesseract()
	{
		// Arrange
		ServiceCollection services = new();
		services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
		services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				[ConfigurationVariables.OcrEngine] = "Tesseract"
			})
			.Build();

		// Act
		InfrastructureService.RegisterOcrEngine(services, configuration);

		// Assert
		ServiceDescriptor? descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IOcrEngine));
		descriptor.Should().NotBeNull();
		descriptor!.ImplementationType.Should().Be(typeof(TesseractOcrEngine));
	}

	[Fact]
	public void RegisterOcrEngine_PaddleOCR_RegistersPaddleOcr()
	{
		// Arrange
		ServiceCollection services = new();
		services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
		services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				[ConfigurationVariables.OcrEngine] = "PaddleOCR"
			})
			.Build();

		// Act
		InfrastructureService.RegisterOcrEngine(services, configuration);

		// Assert
		ServiceDescriptor? descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IOcrEngine));
		descriptor.Should().NotBeNull();
		descriptor!.ImplementationType.Should().Be(typeof(PaddleOcrEngine));
	}

	[Theory]
	[InlineData("paddleocr")]
	[InlineData("PADDLEOCR")]
	[InlineData("PaddleOcr")]
	public void RegisterOcrEngine_PaddleOCR_CaseInsensitive(string engineValue)
	{
		// Arrange
		ServiceCollection services = new();
		services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
		services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				[ConfigurationVariables.OcrEngine] = engineValue
			})
			.Build();

		// Act
		InfrastructureService.RegisterOcrEngine(services, configuration);

		// Assert
		ServiceDescriptor? descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IOcrEngine));
		descriptor.Should().NotBeNull();
		descriptor!.ImplementationType.Should().Be(typeof(PaddleOcrEngine));
	}

	[Theory]
	[InlineData("unknown")]
	[InlineData("")]
	[InlineData("GoogleVision")]
	public void RegisterOcrEngine_UnrecognizedValue_FallsBackToTesseract(string engineValue)
	{
		// Arrange
		ServiceCollection services = new();
		services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
		services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				[ConfigurationVariables.OcrEngine] = engineValue
			})
			.Build();

		// Act
		InfrastructureService.RegisterOcrEngine(services, configuration);

		// Assert — unrecognized values fall back to Tesseract
		ServiceDescriptor? descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IOcrEngine));
		descriptor.Should().NotBeNull();
		descriptor!.ImplementationType.Should().Be(typeof(TesseractOcrEngine));
	}

	#endregion

	#region TesseractOcrEngine.ApplyOcrCorrections delegates to shared helper

	[Fact]
	public void TesseractApplyOcrCorrections_DelegatesToSharedHelper()
	{
		// Verify TesseractOcrEngine.ApplyOcrCorrections still works after refactoring
		string result = TesseractOcrEngine.ApplyOcrCorrections("S1O.5O");
		result.Should().Be("$10.50");
	}

	#endregion
}
