using Application.Models.Ocr;

namespace Application.Commands.Receipt.Scan;

public record ScanReceiptResult(ParsedReceipt ParsedReceipt);
