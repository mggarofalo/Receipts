using Application.Models.Ocr;
using Application.Services.Parsing;
using FluentAssertions;

namespace Application.Tests.Services.Parsing;

public class WalmartReceiptParserTests
{
	private const string SampleOcrText = """
        WAL-MART SUPERCENTER
        Store #1234
        123 Commerce Blvd
        Anytown, US 12345
        ST# 1234 OP# 00567 TE# 12 TR# 09876

        GV WHOLE MILK GL 3.48 O
        GV WHITE BREAD 1.98 O
        BANANAS 1.24
        BOUNTY PAPER TWL 15.97 O
        SAVINGS -2.00

        SUBTOTAL 20.67
        TAX 1 0.97
        TOTAL 21.64

        VISA TEND 21.64
        CHANGE DUE 0.00
        03/15/24  12:34:56

        THANK YOU FOR SHOPPING AT WAL-MART
        """;

	private readonly WalmartReceiptParser _parser = new();

	[Theory]
	[InlineData("WAL-MART SUPERCENTER\nStore #1234", true)]
	[InlineData("WALMART\n123 Main St", true)]
	[InlineData("WAL*MART STORES\nSome address", true)]
	[InlineData("TARGET\n123 Main St", false)]
	[InlineData("COSTCO WHOLESALE\nWarehouse", false)]
	[InlineData("Random Store Receipt", false)]
	public void CanParse_IdentifiesWalmartReceipts(string ocrText, bool expected)
	{
		// Act
		bool actual = _parser.CanParse(ocrText);

		// Assert
		actual.Should().Be(expected);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsStoreName()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.StoreName.Value.Should().Be("Walmart");
		actual.StoreName.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsDate()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Date.Value.Should().Be(new DateOnly(2024, 3, 15));
		actual.Date.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsItems()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Items.Should().HaveCountGreaterThanOrEqualTo(3);
		actual.Items.Should().Contain(i => i.Description.Value == "GV WHOLE MILK GL");
		actual.Items.Should().Contain(i => i.Description.Value == "GV WHITE BREAD");
		actual.Items.Should().Contain(i => i.Description.Value == "BANANAS");
	}

	[Fact]
	public void Parse_SampleReceipt_SkipsSavingsLines()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Items.Should().NotContain(i => i.Description.Value!.Contains("SAVINGS"));
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsTotal()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Total.Value.Should().Be(21.64m);
		actual.Total.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsSubtotal()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Subtotal.Value.Should().Be(20.67m);
		actual.Subtotal.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsTax()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.TaxLines.Should().HaveCount(1);
		actual.TaxLines[0].Amount.Value.Should().Be(0.97m);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsPaymentMethod()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.PaymentMethod.Value.Should().Be("VISA");
		actual.PaymentMethod.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void Parse_TaxLineWithRateAndAmount_ExtractsLastPrice()
	{
		// Arrange
		const string ocrText = """
            WALMART
            Store #1234
            GV MILK 3.48
            SUBTOTAL 3.48
            TAX RATE 8.25% 0.29
            TOTAL 3.77
            VISA TEND 3.77
            03/15/24
            """;

		// Act
		ParsedReceipt actual = _parser.Parse(ocrText);

		// Assert
		actual.TaxLines.Should().HaveCount(1);
		actual.TaxLines[0].Amount.Value.Should().Be(0.29m);
	}

	[Fact]
	public void Parse_SampleReceipt_ItemsHaveHighConfidence()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Items.Should().AllSatisfy(item =>
		{
			item.Description.Confidence.Should().Be(ConfidenceLevel.High);
			item.TotalPrice.Confidence.Should().Be(ConfidenceLevel.High);
		});
	}

	// Regression test for RECEIPTS-609 (supersedes RECEIPTS-607): real-world PaddleOCR
	// output on a Walmart receipt photo contains multi-line text with 12-digit UPCs
	// and inline tax-flag letters. Verify the parser extracts items/subtotal/total/tax/
	// date/payment from this shape — the failure mode before the OCR engine swap was a
	// single run-on line producing zero extraction.
	[Fact]
	public void Parse_RealisticWalmartOcrOutput_ExtractsFields()
	{
		const string ocrText = """
            WALMART SUPERCENTER
            864-834-7179
            Mgr. TERRY
            ST# 05487 OP# 002216 TE# 04 TR# 01841
            # ITEMS SOLD 23
            TC# 7418 8473 9791 0634 4294
            GRANULATED 078742228030 F 3.07
            NCY CRY JA CH 028400589880 F 3.97
            PEANUT BUTTR 051500720020 F 6.97
            SCUR CREAM 073420000110 F 2.64
            FIRE TACO SC 021000046900 F 1.98
            SCRUB SPONGE 051131936820 8.72
            BREAD 072250049190 F 3.76
            BELL PEPPER 881979000870 F 0.82
            BANANAS 000000040110 1.23
            SUBTOTAL 69.68
            TAX 1 6.0000 % 0.75
            TOTAL 70.43
            MASTERCARD TEND 70.43
            CHANGE DUE 0.00
            01/14/26 17:57:23
            """;

		// Act
		ParsedReceipt actual = _parser.Parse(ocrText);

		// Assert — all the things that were zero before the OCR engine swap
		actual.StoreName.Value.Should().Be("Walmart");
		actual.Date.Value.Should().Be(new DateOnly(2026, 1, 14));
		actual.Items.Should().HaveCountGreaterThanOrEqualTo(5);
		actual.Subtotal.Value.Should().Be(69.68m);
		actual.Total.Value.Should().Be(70.43m);
		actual.TaxLines.Should().HaveCount(1);
		actual.TaxLines[0].Amount.Value.Should().Be(0.75m);
		actual.PaymentMethod.Value.Should().Be("MASTERCARD");
	}
}
