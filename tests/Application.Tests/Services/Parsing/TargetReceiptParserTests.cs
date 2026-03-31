using Application.Models.Ocr;
using Application.Services.Parsing;
using FluentAssertions;

namespace Application.Tests.Services.Parsing;

public class TargetReceiptParserTests
{
	private const string SampleOcrText = """
        TARGET
        Store T-2345
        321 Bullseye Blvd
        Shoptown, US 11223

        009-04-0123 HAND SOAP 3.99
        261-02-0456 PAPER PLATES 5.49
        THRESHOLD CANDLE 12.99
        REDCARD SAVINGS -0.65

        SUBTOTAL 21.82
        TAX 1.40
        TOTAL 23.22

        REDCARD ****9876 23.22
        05/18/2024  16:45:33

        EXPECT MORE. PAY LESS.
        """;

	private readonly TargetReceiptParser _parser = new();

	[Theory]
	[InlineData("TARGET\nStore T-2345", true)]
	[InlineData("Welcome to TARGET\n321 Bullseye", true)]
	[InlineData("WALMART\n123 Main St", false)]
	[InlineData("COSTCO\nWarehouse", false)]
	public void CanParse_IdentifiesTargetReceipts(string ocrText, bool expected)
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
		actual.StoreName.Value.Should().Be("Target");
		actual.StoreName.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsDate()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Date.Value.Should().Be(new DateOnly(2024, 5, 18));
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsItemsWithDpci()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Items.Should().Contain(i =>
			i.Description.Value == "HAND SOAP" &&
			i.Code.Value == "009-04-0123" &&
			i.TotalPrice.Value == 3.99m);

		actual.Items.Should().Contain(i =>
			i.Description.Value == "PAPER PLATES" &&
			i.Code.Value == "261-02-0456" &&
			i.TotalPrice.Value == 5.49m);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsItemWithoutDpci()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Items.Should().Contain(i => i.Description.Value == "THRESHOLD CANDLE" && i.TotalPrice.Value == 12.99m);
	}

	[Fact]
	public void Parse_SampleReceipt_SkipsRedcardSavingsLine()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Items.Should().NotContain(i => i.Description.Value!.Contains("REDCARD SAVINGS"));
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsTotal()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Total.Value.Should().Be(23.22m);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsSubtotal()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Subtotal.Value.Should().Be(21.82m);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsTax()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.TaxLines.Should().HaveCount(1);
		actual.TaxLines[0].Amount.Value.Should().Be(1.40m);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsPaymentMethod()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.PaymentMethod.Value.Should().Be("REDCARD");
		actual.PaymentMethod.Confidence.Should().Be(ConfidenceLevel.High);
	}
}
