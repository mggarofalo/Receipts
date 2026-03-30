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
}
