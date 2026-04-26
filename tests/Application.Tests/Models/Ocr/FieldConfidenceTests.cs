using Application.Models.Ocr;
using FluentAssertions;

namespace Application.Tests.Models.Ocr;

/// <summary>
/// Verifies the "absent vs. present" semantics of <see cref="FieldConfidence{T}"/>.
/// RECEIPTS-631 split the previous overloaded <c>None() == Low</c> sentinel into
/// distinct <see cref="ConfidenceLevel.None"/> and <see cref="ConfidenceLevel.Low"/>
/// states; these tests pin the new contract so a future regression cannot silently
/// merge the two again.
/// </summary>
public class FieldConfidenceTests
{
	[Fact]
	public void None_ProducesDefaultValue_WithNoneConfidence()
	{
		FieldConfidence<string> f = FieldConfidence<string>.None();

		f.Value.Should().BeNull();
		f.Confidence.Should().Be(ConfidenceLevel.None);
	}

	[Fact]
	public void None_OnValueType_ProducesDefaultWithNoneConfidence()
	{
		FieldConfidence<decimal> f = FieldConfidence<decimal>.None();

		// Value is the CLR default (0m) but Confidence is None — distinct from Low(0m).
		f.Value.Should().Be(0m);
		f.Confidence.Should().Be(ConfidenceLevel.None);
	}

	[Fact]
	public void Low_WithRealValue_IsDistinctFromNone()
	{
		FieldConfidence<string> low = FieldConfidence<string>.Low("Walmart");
		FieldConfidence<string> none = FieldConfidence<string>.None();

		low.Should().NotBe(none);
		low.Confidence.Should().Be(ConfidenceLevel.Low);
		low.Value.Should().Be("Walmart");
		none.Confidence.Should().Be(ConfidenceLevel.None);
	}

	[Fact]
	public void Low_WithDefaultValue_IsStillDistinctFromNone()
	{
		// A deliberate Low(0m) — meaning the VLM extracted "$0.00" with low confidence —
		// must remain distinguishable from None() (the field was not extracted at all).
		FieldConfidence<decimal> lowZero = FieldConfidence<decimal>.Low(0m);
		FieldConfidence<decimal> none = FieldConfidence<decimal>.None();

		lowZero.Should().NotBe(none);
		lowZero.Confidence.Should().Be(ConfidenceLevel.Low);
		none.Confidence.Should().Be(ConfidenceLevel.None);
	}

	[Fact]
	public void IsPresent_True_ForLowMediumHigh()
	{
		FieldConfidence<string>.Low("v").IsPresent.Should().BeTrue();
		FieldConfidence<string>.Medium("v").IsPresent.Should().BeTrue();
		FieldConfidence<string>.High("v").IsPresent.Should().BeTrue();
	}

	[Fact]
	public void IsPresent_False_ForNone()
	{
		FieldConfidence<string>.None().IsPresent.Should().BeFalse();
		FieldConfidence<decimal>.None().IsPresent.Should().BeFalse();
		FieldConfidence<DateOnly>.None().IsPresent.Should().BeFalse();
	}

	[Fact]
	public void DefaultEnumValue_IsNone()
	{
		// Defensive: a freshly default-initialized ConfidenceLevel must read as None,
		// not Low. This guards against silent misclassification of zero-initialized
		// values reaching production code as "low confidence" extractions.
		ConfidenceLevel defaultLevel = default;

		defaultLevel.Should().Be(ConfidenceLevel.None);
	}
}
