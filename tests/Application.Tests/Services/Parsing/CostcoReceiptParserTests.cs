using Application.Models.Ocr;
using Application.Services.Parsing;
using FluentAssertions;

namespace Application.Tests.Services.Parsing;

public class CostcoReceiptParserTests
{
	private const string SampleOcrText = """
        COSTCO WHOLESALE
        Warehouse #789
        456 Bulk Lane
        Membership #1234567890

        1234567 KS OLIVE OIL 12.99
        9876543 ORGANIC EGGS 6.49
        5551234 KS PAPER TOWELS 18.99
        ROTISSERIE CHICKEN 4.99

        SUBTOTAL 43.46
        TAX 1.82
        TOTAL 45.28

        VISA ****1234 45.28
        06/22/2024  14:30:00

        THANK YOU
        """;

	private readonly CostcoReceiptParser _parser = new();

	[Theory]
	[InlineData("COSTCO WHOLESALE\nWarehouse #789", true)]
	[InlineData("COSTCO\n456 Bulk Lane", true)]
	[InlineData("WALMART\n123 Main St", false)]
	[InlineData("TARGET\nStore #99", false)]
	public void CanParse_IdentifiesCostcoReceipts(string ocrText, bool expected)
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
		actual.StoreName.Value.Should().Be("Costco");
		actual.StoreName.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsDate()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Date.Value.Should().Be(new DateOnly(2024, 6, 22));
		actual.Date.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsItemsWithCodes()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Items.Should().HaveCountGreaterThanOrEqualTo(3);
		actual.Items.Should().Contain(i => i.Description.Value == "KS OLIVE OIL" && i.Code.Value == "1234567");
		actual.Items.Should().Contain(i => i.Description.Value == "ORGANIC EGGS" && i.Code.Value == "9876543");
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsItemWithoutCode()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Items.Should().Contain(i => i.Description.Value == "ROTISSERIE CHICKEN");
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsTotal()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Total.Value.Should().Be(45.28m);
		actual.Total.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsSubtotal()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.Subtotal.Value.Should().Be(43.46m);
	}

	[Fact]
	public void Parse_SampleReceipt_ExtractsTax()
	{
		// Act
		ParsedReceipt actual = _parser.Parse(SampleOcrText);

		// Assert
		actual.TaxLines.Should().HaveCount(1);
		actual.TaxLines[0].Amount.Value.Should().Be(1.82m);
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
