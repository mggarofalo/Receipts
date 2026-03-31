using Application.Interfaces.Services;
using Application.Models.Ocr;
using Application.Services;
using FluentAssertions;
using Moq;

namespace Application.Tests.Services;

public class ReceiptParsingServiceTests
{
	[Fact]
	public void Parse_FirstMatchingParserWins()
	{
		// Arrange
		ParsedReceipt expected = CreateMinimalReceipt("First Parser");
		Mock<IReceiptParser> firstParser = new();
		firstParser.Setup(p => p.CanParse(It.IsAny<string>())).Returns(true);
		firstParser.Setup(p => p.Parse(It.IsAny<string>())).Returns(expected);

		Mock<IReceiptParser> secondParser = new();
		secondParser.Setup(p => p.CanParse(It.IsAny<string>())).Returns(true);
		secondParser.Setup(p => p.Parse(It.IsAny<string>())).Returns(CreateMinimalReceipt("Second Parser"));

		ReceiptParsingService service = new([firstParser.Object, secondParser.Object]);

		// Act
		ParsedReceipt actual = service.Parse("some receipt text");

		// Assert
		actual.StoreName.Value.Should().Be("First Parser");
		secondParser.Verify(p => p.Parse(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void Parse_SkipsNonMatchingParsers()
	{
		// Arrange
		Mock<IReceiptParser> nonMatchingParser = new();
		nonMatchingParser.Setup(p => p.CanParse(It.IsAny<string>())).Returns(false);

		ParsedReceipt expected = CreateMinimalReceipt("Fallback");
		Mock<IReceiptParser> fallbackParser = new();
		fallbackParser.Setup(p => p.CanParse(It.IsAny<string>())).Returns(true);
		fallbackParser.Setup(p => p.Parse(It.IsAny<string>())).Returns(expected);

		ReceiptParsingService service = new([nonMatchingParser.Object, fallbackParser.Object]);

		// Act
		ParsedReceipt actual = service.Parse("some receipt text");

		// Assert
		actual.StoreName.Value.Should().Be("Fallback");
		nonMatchingParser.Verify(p => p.Parse(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public void Parse_ThrowsWhenNoParsersMatch()
	{
		// Arrange
		Mock<IReceiptParser> parser = new();
		parser.Setup(p => p.CanParse(It.IsAny<string>())).Returns(false);

		ReceiptParsingService service = new([parser.Object]);

		// Act
		Action act = () => service.Parse("some receipt text");

		// Assert
		act.Should().Throw<InvalidOperationException>()
			.WithMessage("*No receipt parser*");
	}

	[Fact]
	public void Parse_RegistrationOrderRespected()
	{
		// Arrange
		Mock<IReceiptParser> parserA = new();
		parserA.Setup(p => p.CanParse(It.IsAny<string>())).Returns(false);

		Mock<IReceiptParser> parserB = new();
		parserB.Setup(p => p.CanParse(It.IsAny<string>())).Returns(true);
		parserB.Setup(p => p.Parse(It.IsAny<string>())).Returns(CreateMinimalReceipt("B"));

		Mock<IReceiptParser> parserC = new();
		parserC.Setup(p => p.CanParse(It.IsAny<string>())).Returns(true);
		parserC.Setup(p => p.Parse(It.IsAny<string>())).Returns(CreateMinimalReceipt("C"));

		ReceiptParsingService service = new([parserA.Object, parserB.Object, parserC.Object]);

		// Act
		ParsedReceipt actual = service.Parse("text");

		// Assert
		actual.StoreName.Value.Should().Be("B");
		parserA.Verify(p => p.CanParse("text"), Times.Once);
		parserB.Verify(p => p.CanParse("text"), Times.Once);
		parserC.Verify(p => p.CanParse(It.IsAny<string>()), Times.Never);
	}

	private static ParsedReceipt CreateMinimalReceipt(string storeName)
	{
		return new ParsedReceipt(
			StoreName: FieldConfidence<string>.High(storeName),
			Date: FieldConfidence<DateOnly>.None(),
			Items: [],
			Subtotal: FieldConfidence<decimal>.None(),
			TaxLines: [],
			Total: FieldConfidence<decimal>.None(),
			PaymentMethod: FieldConfidence<string?>.None()
		);
	}
}
