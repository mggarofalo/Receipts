using Application.Models.Ocr;
using Application.Services.Parsing;
using FluentAssertions;

namespace Application.Tests.Services.Parsing;

public class AldiReceiptParserTests
{
	private const string SampleOcrText = """
        ALDI
        Store #567
        789 Value Street
        Budget City, US 67890

        HAPPY FARMS MILK 2.89
        L'OVEN FRESH BREAD 1.39
        SWEET POTATOES 2.49
        REGGANO PASTA 0.95

        SUBTOTAL 7.72
        TAX 0.62
        TOTAL 8.34

        DEBIT CARD 8.34
        04/10/2024  09:15:22

        THANK YOU FOR SHOPPING AT ALDI
        """;

	private readonly AldiReceiptParser _parser = new();

	[Theory]
	[InlineData("ALDI\nStore #567", true)]
	[InlineData("Welcome to ALDI\n789 Value St", true)]
	[InlineData("WALMART\n123 Main St", false)]
	[InlineData("ALDIS DISCOUNT STORE", false)]
	public void CanParse_IdentifiesAldiReceipts(string ocrText, bool expected)
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
		actual.StoreName.Value.Should().Be("ALDI");
		actual.StoreName.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsDate()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Date.Value.Should().Be(new DateOnly(2024, 4, 10));
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsItems()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Items.Should().HaveCountGreaterThanOrEqualTo(4);
		actual.Items.Should().Contain(i => i.Description.Value == "HAPPY FARMS MILK" && i.TotalPrice.Value == 2.89m);
		actual.Items.Should().Contain(i => i.Description.Value == "L'OVEN FRESH BREAD" && i.TotalPrice.Value == 1.39m);
		actual.Items.Should().Contain(i => i.Description.Value == "SWEET POTATOES" && i.TotalPrice.Value == 2.49m);
		actual.Items.Should().Contain(i => i.Description.Value == "REGGANO PASTA" && i.TotalPrice.Value == 0.95m);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsTotal()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Total.Value.Should().Be(8.34m);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsSubtotal()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Subtotal.Value.Should().Be(7.72m);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsTax()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.TaxLines.Should().HaveCount(1);
		actual.TaxLines[0].Amount.Value.Should().Be(0.62m);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsPaymentMethod()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.PaymentMethod.Value.Should().Be("DEBIT");
		actual.PaymentMethod.Confidence.Should().Be(ConfidenceLevel.Medium);
	}
}
