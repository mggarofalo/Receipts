using Application.Models.Ocr;
using Application.Services.Parsing;
using FluentAssertions;

namespace Application.Tests.Services.Parsing;

public class GenericReceiptParserTests
{
	private readonly GenericReceiptParser _parser = new();

	[Fact]
	public void CanParse_AlwaysReturnsTrue()
	{
		// Arrange & Act & Assert
		_parser.CanParse("any text").Should().BeTrue();
		_parser.CanParse("").Should().BeTrue();
		_parser.CanParse("WALMART").Should().BeTrue();
	}

	[Fact]
	public void Parse_EmptyInput_ReturnsReceiptWithDefaults()
	{
		// Arrange & Act
		ParsedReceipt actual = _parser.Parse("");

		// Assert
		actual.StoreName.Confidence.Should().Be(ConfidenceLevel.Low);
		actual.Date.Confidence.Should().Be(ConfidenceLevel.Low);
		actual.Items.Should().BeEmpty();
		actual.Subtotal.Confidence.Should().Be(ConfidenceLevel.Low);
		actual.TaxLines.Should().BeEmpty();
		actual.Total.Confidence.Should().Be(ConfidenceLevel.Low);
		actual.PaymentMethod.Confidence.Should().Be(ConfidenceLevel.Low);
	}

	[Fact]
	public void Parse_WhitespaceInput_ReturnsReceiptWithDefaults()
	{
		// Arrange & Act
		ParsedReceipt actual = _parser.Parse("   \n   \n   ");

		// Assert
		actual.Items.Should().BeEmpty();
		actual.Total.Confidence.Should().Be(ConfidenceLevel.Low);
	}

	[Theory]
	[InlineData("Date: 2024-03-15", 2024, 3, 15, ConfidenceLevel.High)]
	[InlineData("03/15/2024", 2024, 3, 15, ConfidenceLevel.Medium)]
	[InlineData("03/15/24", 2024, 3, 15, ConfidenceLevel.Medium)]
	[InlineData("March 15, 2024", 2024, 3, 15, ConfidenceLevel.Medium)]
	public void Parse_ExtractsDateInMultipleFormats(string input, int expectedYear, int expectedMonth, int expectedDay, ConfidenceLevel expectedConfidence)
	{
		// Arrange
		DateOnly expectedDate = new(expectedYear, expectedMonth, expectedDay);

		// Act
		ParsedReceipt actual = _parser.Parse(input);

		// Assert
		actual.Date.Value.Should().Be(expectedDate);
		actual.Date.Confidence.Should().Be(expectedConfidence);
	}

	[Fact]
	public void Parse_ExtractsPricesAsItems()
	{
		// Arrange
		const string ocrText = """
            Some Store
            Milk 2% Gallon 3.49
            Bread Wheat 2.99
            SUBTOTAL 6.48
            TAX 0.52
            TOTAL 7.00
            """;

		// Act
		ParsedReceipt actual = _parser.Parse(ocrText);

		// Assert
		actual.Items.Should().HaveCount(2);
		actual.Items[0].Description.Value.Should().Be("Milk 2% Gallon");
		actual.Items[0].TotalPrice.Value.Should().Be(3.49m);
		actual.Items[1].Description.Value.Should().Be("Bread Wheat");
		actual.Items[1].TotalPrice.Value.Should().Be(2.99m);
	}

	[Fact]
	public void Parse_ExtractsTotal()
	{
		// Arrange
		const string ocrText = """
            Item 5.00
            TOTAL 5.00
            """;

		// Act
		ParsedReceipt actual = _parser.Parse(ocrText);

		// Assert
		actual.Total.Value.Should().Be(5.00m);
		actual.Total.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void Parse_ExtractsGrandTotal()
	{
		// Arrange
		const string ocrText = """
            Item 5.00
            GRAND TOTAL 5.00
            """;

		// Act
		ParsedReceipt actual = _parser.Parse(ocrText);

		// Assert
		actual.Total.Value.Should().Be(5.00m);
		actual.Total.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void Parse_ExtractsSubtotal()
	{
		// Arrange
		const string ocrText = """
            Item 5.00
            SUBTOTAL 5.00
            TAX 0.40
            TOTAL 5.40
            """;

		// Act
		ParsedReceipt actual = _parser.Parse(ocrText);

		// Assert
		actual.Subtotal.Value.Should().Be(5.00m);
		actual.Subtotal.Confidence.Should().Be(ConfidenceLevel.High);
	}

	[Fact]
	public void Parse_ExtractsTaxLines()
	{
		// Arrange
		const string ocrText = """
            Item 10.00
            SUBTOTAL 10.00
            TAX 0.80
            TOTAL 10.80
            """;

		// Act
		ParsedReceipt actual = _parser.Parse(ocrText);

		// Assert
		actual.TaxLines.Should().HaveCount(1);
		actual.TaxLines[0].Amount.Value.Should().Be(0.80m);
	}

	[Theory]
	[InlineData("VISA ending 1234", "VISA")]
	[InlineData("MASTERCARD ****5678", "MASTERCARD")]
	[InlineData("AMEX ending 0001", "AMEX")]
	[InlineData("AMERICAN EXPRESS", "AMEX")]
	[InlineData("DEBIT CARD", "DEBIT")]
	[InlineData("CREDIT CARD", "CREDIT")]
	[InlineData("CASH TENDERED", "CASH")]
	public void Parse_ExtractsPaymentMethod(string paymentLine, string expectedMethod)
	{
		// Arrange
		string ocrText = $"Store\nItem 5.00\nTOTAL 5.00\n{paymentLine}";

		// Act
		ParsedReceipt actual = _parser.Parse(ocrText);

		// Assert
		actual.PaymentMethod.Value.Should().Be(expectedMethod);
	}

	[Fact]
	public void Parse_StoreNameIsFirstNonBlankNonSpecialLine()
	{
		// Arrange
		const string ocrText = """
            MEGAMART
            123 Main St
            Item A 3.00
            TOTAL 3.00
            """;

		// Act
		ParsedReceipt actual = _parser.Parse(ocrText);

		// Assert
		actual.StoreName.Value.Should().Be("MEGAMART");
		actual.StoreName.Confidence.Should().Be(ConfidenceLevel.Low);
	}

	[Fact]
	public void Parse_DoesNotCountTotalAsTaxLineOrItem()
	{
		// Arrange
		const string ocrText = """
            Store
            Apple 1.50
            TOTAL 1.50
            """;

		// Act
		ParsedReceipt actual = _parser.Parse(ocrText);

		// Assert
		actual.Items.Should().HaveCount(1);
		actual.Items[0].Description.Value.Should().Be("Apple");
		actual.TaxLines.Should().BeEmpty();
	}

	[Fact]
	public void Parse_TaxLineWithRateAndAmount_ExtractsLastPrice()
	{
		// Arrange
		const string ocrText = """
            Store
            Item 10.00
            SUBTOTAL 10.00
            TAX RATE 8.25% 0.83
            TOTAL 10.83
            """;

		// Act
		ParsedReceipt actual = _parser.Parse(ocrText);

		// Assert
		actual.TaxLines.Should().HaveCount(1);
		actual.TaxLines[0].Amount.Value.Should().Be(0.83m);
	}

	[Fact]
	public void Parse_TotalLineWithMultipleDecimals_ExtractsLastPrice()
	{
		// Arrange
		const string ocrText = """
            Store
            Item 5.00
            SUBTOTAL 5.00
            TAX 0.40
            TOTAL DUE 5.40% 5.40
            """;

		// Act
		ParsedReceipt actual = _parser.Parse(ocrText);

		// Assert
		actual.Total.Value.Should().Be(5.40m);
	}

	[Fact]
	public void Parse_SubtotalLineWithMultipleDecimals_ExtractsLastPrice()
	{
		// Arrange
		const string ocrText = """
            Store
            Item 5.00
            SUBTOTAL ITEMS 3.00 5.00
            TAX 0.40
            TOTAL 5.40
            """;

		// Act
		ParsedReceipt actual = _parser.Parse(ocrText);

		// Assert
		actual.Subtotal.Value.Should().Be(5.00m);
	}

	[Fact]
	public void Parse_FullReceipt_IntegrationStyle()
	{
		// Arrange
		const string ocrText = """
            NEIGHBORHOOD GROCERY
            456 Oak Avenue
            Date: 2024-06-20

            Bananas 0.79
            Chicken Breast 7.49
            Rice 5lb Bag 4.99
            Olive Oil 8.99

            SUBTOTAL 22.26
            TAX 1.78
            TOTAL 24.04

            VISA ending 4321
            THANK YOU FOR SHOPPING
            """;

		// Act
		ParsedReceipt actual = _parser.Parse(ocrText);

		// Assert
		actual.StoreName.Value.Should().Be("NEIGHBORHOOD GROCERY");
		actual.Date.Value.Should().Be(new DateOnly(2024, 6, 20));
		actual.Items.Should().HaveCount(4);
		actual.Subtotal.Value.Should().Be(22.26m);
		actual.TaxLines.Should().HaveCount(1);
		actual.TaxLines[0].Amount.Value.Should().Be(1.78m);
		actual.Total.Value.Should().Be(24.04m);
		actual.PaymentMethod.Value.Should().Be("VISA");
	}
}
