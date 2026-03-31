namespace Application.Interfaces.Services;

using Application.Models.Ocr;

public interface IReceiptParser
{
	bool CanParse(string ocrText);
	ParsedReceipt Parse(string ocrText);
}

public interface IReceiptParsingService
{
	ParsedReceipt Parse(string ocrText);
}
