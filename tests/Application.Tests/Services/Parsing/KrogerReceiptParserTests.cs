using Application.Models.Ocr;
using Application.Services.Parsing;
using FluentAssertions;

namespace Application.Tests.Services.Parsing;

public class KrogerReceiptParserTests
{
	private const string SampleOcrText = """
        KROGER
        Store #0456
        555 Fresh Way
        Groceryville, US 33445

        KROGER MILK 1GAL 3.29
        WHEAT BREAD 2.49
        CHICKEN THIGHS 6.99
        DIGITAL COUPON -1.00
        FUEL POINTS EARNED: 45

        SUBTOTAL 11.77
        TAX 0.73
        TOTAL 12.50

        VISA ****4567 12.50
        07/04/2024  11:22:33

        FUEL POINTS BALANCE: 245
        THANK YOU FOR SHOPPING KROGER
        """;

	private readonly KrogerReceiptParser _parser = new();

	[Theory]
	[InlineData("KROGER\nStore #0456", true)]
	[InlineData("Welcome to KROGER\n555 Fresh Way", true)]
	[InlineData("FRED MEYER\nStore #789", true)]
	[InlineData("WALMART\n123 Main St", false)]
	[InlineData("TARGET\nStore #99", false)]
	public void CanParse_IdentifiesKrogerReceipts(string ocrText, bool expected)
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
		actual.StoreName.Value.Should().Be("Kroger");
		actual.StoreName.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsDate()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Date.Value.Should().Be(new DateOnly(2024, 7, 4));
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsItems()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Items.Should().Contain(i => i.Description.Value == "KROGER MILK 1GAL" && i.TotalPrice.Value == 3.29m);
		actual.Items.Should().Contain(i => i.Description.Value == "WHEAT BREAD" && i.TotalPrice.Value == 2.49m);
		actual.Items.Should().Contain(i => i.Description.Value == "CHICKEN THIGHS" && i.TotalPrice.Value == 6.99m);
	}

	[Fact]
	public void Parse_SampleReceipt_SkipsFuelPointsAndCouponLines()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Items.Should().NotContain(i => i.Description.Value!.Contains("FUEL POINTS"));
		actual.Items.Should().NotContain(i => i.Description.Value!.Contains("DIGITAL COUPON"));
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsTotal()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Total.Value.Should().Be(12.50m);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsSubtotal()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Subtotal.Value.Should().Be(11.77m);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsTax()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.TaxLines.Should().HaveCount(1);
		actual.TaxLines[0].Amount.Value.Should().Be(0.73m);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsPaymentMethod()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.PaymentMethod.Value.Should().Be("VISA");
	}
}
