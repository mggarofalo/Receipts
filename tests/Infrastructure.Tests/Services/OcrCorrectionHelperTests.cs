using FluentAssertions;
using Infrastructure.Services;

namespace Infrastructure.Tests.Services;

public class OcrCorrectionHelperTests
{
	[Theory]
	[InlineData("S12.99", "$12.99")]
	[InlineData("S5.00", "$5.00")]
	[InlineData("Total S12.99", "Total $12.99")]
	public void ApplyOcrCorrections_DollarSign_ReplacesSAtStartOrAfterSpace(string input, string expected)
	{
		string actual = OcrCorrectionHelper.ApplyOcrCorrections(input);
		actual.Should().Be(expected);
	}

	[Theory]
	[InlineData("512.99", "512.99")]
	[InlineData("5.00", "5.00")]
	[InlineData("COSTCO", "COSTCO")]
	public void ApplyOcrCorrections_DollarSign_LeavesNonSContextAlone(string input, string expected)
	{
		string actual = OcrCorrectionHelper.ApplyOcrCorrections(input);
		actual.Should().Be(expected);
	}

	[Theory]
	[InlineData("1O.5O", "10.50")]
	[InlineData("2O.OO", "20.00")]
	[InlineData("1OO", "100")]
	public void ApplyOcrCorrections_OToZero_ReplacesInDigitContext(string input, string expected)
	{
		string actual = OcrCorrectionHelper.ApplyOcrCorrections(input);
		actual.Should().Be(expected);
	}

	[Fact]
	public void ApplyOcrCorrections_OToZero_LeavesNonDigitContextAlone()
	{
		string actual = OcrCorrectionHelper.ApplyOcrCorrections("TOTAL AMOUNT");
		actual.Should().Be("TOTAL AMOUNT");
	}

	[Theory]
	[InlineData("l2.00", "12.00")]
	[InlineData("I5.99", "15.99")]
	[InlineData("3l.50", "31.50")]
	public void ApplyOcrCorrections_LAndIToOne_ReplacesInDigitContext(string input, string expected)
	{
		string actual = OcrCorrectionHelper.ApplyOcrCorrections(input);
		actual.Should().Be(expected);
	}

	[Theory]
	[InlineData("ITEM", "ITEM")]
	[InlineData("line", "line")]
	[InlineData("Illinois", "Illinois")]
	public void ApplyOcrCorrections_LAndIToOne_LeavesNonDigitContextAlone(string input, string expected)
	{
		string actual = OcrCorrectionHelper.ApplyOcrCorrections(input);
		actual.Should().Be(expected);
	}

	[Fact]
	public void ApplyOcrCorrections_LineNormalization_TrimsAndCollapsesBlankLines()
	{
		string input = "Line 1  \n\n\n\nLine 2  \n\n\n\n\nLine 3";
		string actual = OcrCorrectionHelper.ApplyOcrCorrections(input);
		actual.Should().Be("Line 1\n\nLine 2\n\nLine 3");
	}

	[Fact]
	public void ApplyOcrCorrections_NoCorrections_ReturnsUnchanged()
	{
		string input = "$12.99\nBananas\n$3.50";
		string actual = OcrCorrectionHelper.ApplyOcrCorrections(input);
		actual.Should().Be(input);
	}

	[Fact]
	public void ApplyOcrCorrections_EmptyString_ReturnsEmpty()
	{
		string actual = OcrCorrectionHelper.ApplyOcrCorrections(string.Empty);
		actual.Should().BeEmpty();
	}

	[Fact]
	public void ApplyOcrCorrections_NullString_ReturnsNull()
	{
		string actual = OcrCorrectionHelper.ApplyOcrCorrections(null!);
		actual.Should().BeNull();
	}

	[Theory]
	[InlineData("S1.00 S2.00", "$1.00 $2.00")]
	[InlineData("Subtotal S15.99\nTax S1.28\nTotal S17.27", "Subtotal $15.99\nTax $1.28\nTotal $17.27")]
	public void ApplyOcrCorrections_MultipleDollarSigns_ReplacesAll(string input, string expected)
	{
		string actual = OcrCorrectionHelper.ApplyOcrCorrections(input);
		actual.Should().Be(expected);
	}

	[Fact]
	public void ApplyOcrCorrections_CombinedCorrections_AppliesAll()
	{
		string input = "S1O.5O\nI2.OO\nl5.99";
		string actual = OcrCorrectionHelper.ApplyOcrCorrections(input);
		actual.Should().Be("$10.50\n12.00\n15.99");
	}
}
