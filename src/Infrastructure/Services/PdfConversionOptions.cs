using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services;

/// <summary>
/// Tunable thresholds for <see cref="PdfConversionService"/>. Bound from the
/// <c>PdfConversion</c> configuration section via
/// <see cref="Microsoft.Extensions.DependencyInjection.OptionsConfigurationServiceCollectionExtensions"/>
/// and validated at startup (RECEIPTS-638). Values are policy that may legitimately differ
/// per environment, so they live here rather than as <c>const</c>s on either the interface
/// or the service implementation.
/// </summary>
public sealed class PdfConversionOptions
{
	/// <summary>
	/// Maximum number of pages allowed in a single PDF upload. PDFs with more pages are
	/// rejected before rasterization to keep hostile or accidental uploads from spending
	/// rasterization budget on work we will reject anyway. Default 50 matches the historical
	/// hardcoded value that previously lived on <c>IPdfConversionService</c>.
	/// </summary>
	[Range(1, 10_000)]
	public int MaxPages { get; set; } = 50;
}
