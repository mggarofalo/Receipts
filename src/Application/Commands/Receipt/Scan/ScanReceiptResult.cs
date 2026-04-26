using Application.Models.Ocr;

namespace Application.Commands.Receipt.Scan;

/// <summary>
/// Result of a receipt scan.
/// </summary>
/// <param name="ParsedReceipt">The receipt extracted from the source file.</param>
/// <param name="DroppedPageCount">
/// Number of source pages that were silently ignored during extraction. For PDFs,
/// only the first page is rasterized and sent to the VLM; pages 2..N are dropped.
/// This count lets callers warn the user that the proposal represents only part of
/// the document. Always 0 for single-page sources (images or single-page PDFs).
/// </param>
public record ScanReceiptResult(ParsedReceipt ParsedReceipt, int DroppedPageCount = 0);
