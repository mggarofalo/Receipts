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

	#region ReconstructLines

	[Fact]
	public void ReconstructLines_NoRegions_ReturnsEmpty()
	{
		string actual = PaddleOcrEngine.ReconstructLines([]);
		actual.Should().BeEmpty();
	}

	[Fact]
	public void ReconstructLines_GroupsRegionsOnSameY_JoinsWithSpace()
	{
		// Three regions on the same row (Y=50±2, height=10), in random input
		// order — should emerge sorted by X on a single line.
		string actual = PaddleOcrEngine.ReconstructLines(
			[
				(X: 200, Y: 50, Height: 10, Text: "FLAG"),
				(X: 50, Y: 51, Height: 10, Text: "DESC"),
				(X: 300, Y: 49, Height: 10, Text: "3.07"),
			]);
		actual.Should().Be("DESC FLAG 3.07");
	}

	[Fact]
	public void ReconstructLines_SeparatesRowsByYTolerance()
	{
		// Row 1 at Y=50, Row 2 at Y=70 (delta 20 > half of median height 10 = 5).
		string actual = PaddleOcrEngine.ReconstructLines(
			[
				(X: 50, Y: 50, Height: 10, Text: "GRANULATED"),
				(X: 200, Y: 50, Height: 10, Text: "3.07"),
				(X: 50, Y: 70, Height: 10, Text: "BANANAS"),
				(X: 200, Y: 70, Height: 10, Text: "1.23"),
			]);
		actual.Should().Be("GRANULATED 3.07\nBANANAS 1.23");
	}

	[Fact]
	public void ReconstructLines_ReconstructsWalmartItemLineFromWordFragments()
	{
		// Simulates Paddle returning each part of a Walmart item line
		// as a separate region: DESC | UPC | FLAG | PRICE.
		string actual = PaddleOcrEngine.ReconstructLines(
			[
				(X: 50, Y: 100, Height: 18, Text: "GRANULATED"),
				(X: 180, Y: 98, Height: 18, Text: "078742228030"),
				(X: 340, Y: 102, Height: 18, Text: "F"),
				(X: 400, Y: 101, Height: 18, Text: "3.07"),
			]);
		actual.Should().Be("GRANULATED 078742228030 F 3.07");
	}

	[Fact]
	public void ReconstructLines_SkipsEmptyTextRegions()
	{
		string actual = PaddleOcrEngine.ReconstructLines(
			[
				(X: 50, Y: 50, Height: 10, Text: "HELLO"),
				(X: 150, Y: 50, Height: 10, Text: ""),
				(X: 250, Y: 50, Height: 10, Text: "WORLD"),
			]);
		actual.Should().Be("HELLO WORLD");
	}

	[Fact]
	public void ReconstructLines_PreservesTopToBottomOrder()
	{
		// Three rows in reverse-input order; output must be top-down.
		string actual = PaddleOcrEngine.ReconstructLines(
			[
				(X: 50, Y: 300, Height: 15, Text: "bottom"),
				(X: 50, Y: 100, Height: 15, Text: "top"),
				(X: 50, Y: 200, Height: 15, Text: "middle"),
			]);
		actual.Should().Be("top\nmiddle\nbottom");
	}

	#endregion

	#region ComputeWeightedConfidence

	[Fact]
	public void ComputeWeightedConfidence_NoRegions_ReturnsZero()
	{
		float actual = PaddleOcrEngine.ComputeWeightedConfidence([]);
		actual.Should().Be(0f);
	}

	[Fact]
	public void ComputeWeightedConfidence_WeightsByTextLength()
	{
		// Region A: score 0.9, length 10 → contributes 9.0
		// Region B: score 0.5, length 2  → contributes 1.0
		// Total weighted: 10.0, total length: 12 → 10/12 ≈ 0.833
		float actual = PaddleOcrEngine.ComputeWeightedConfidence(
			[(0.9f, 10), (0.5f, 2)]);
		actual.Should().BeApproximately(10f / 12f, 0.0001f);
	}

	[Fact]
	public void ComputeWeightedConfidence_SkipsNaNScores()
	{
		// One valid region (0.95, length 5) and one NaN region should yield 0.95,
		// not NaN — JSON serialization rejects NaN/Infinity.
		float actual = PaddleOcrEngine.ComputeWeightedConfidence(
			[(0.95f, 5), (float.NaN, 10)]);
		actual.Should().BeApproximately(0.95f, 0.0001f);
		float.IsFinite(actual).Should().BeTrue();
	}

	[Fact]
	public void ComputeWeightedConfidence_SkipsInfiniteScores()
	{
		float actual = PaddleOcrEngine.ComputeWeightedConfidence(
			[(0.8f, 5), (float.PositiveInfinity, 10), (float.NegativeInfinity, 3)]);
		actual.Should().BeApproximately(0.8f, 0.0001f);
		float.IsFinite(actual).Should().BeTrue();
	}

	[Fact]
	public void ComputeWeightedConfidence_AllScoresInvalid_ReturnsZero()
	{
		float actual = PaddleOcrEngine.ComputeWeightedConfidence(
			[(float.NaN, 5), (float.PositiveInfinity, 10)]);
		actual.Should().Be(0f);
	}

	[Fact]
	public void ComputeWeightedConfidence_ClampsAbove1()
	{
		// A score >1 is not normal for Paddle but defensively clamped so we
		// never emit >1 into a confidence field downstream.
		float actual = PaddleOcrEngine.ComputeWeightedConfidence(
			[(1.5f, 5)]);
		actual.Should().Be(1f);
	}

	[Fact]
	public void ComputeWeightedConfidence_ClampsBelow0()
	{
		float actual = PaddleOcrEngine.ComputeWeightedConfidence(
			[(-0.3f, 5)]);
		actual.Should().Be(0f);
	}

	#endregion
}
