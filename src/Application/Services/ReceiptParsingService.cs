using Application.Interfaces.Services;
using Application.Models.Ocr;

namespace Application.Services;

public class ReceiptParsingService(IEnumerable<IReceiptParser> parsers) : IReceiptParsingService
{
	public ParsedReceipt Parse(string ocrText)
	{
		foreach (IReceiptParser parser in parsers)
		{
			if (parser.CanParse(ocrText))
			{
				return parser.Parse(ocrText);
			}
		}

		throw new InvalidOperationException("No receipt parser could handle the provided OCR text.");
	}
}
