using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services;

/// <summary>
/// Tunable thresholds for <see cref="ImageValidationService"/>. Bound from the
/// <c>ImageValidation</c> configuration section via
/// <see cref="Microsoft.Extensions.DependencyInjection.OptionsConfigurationServiceCollectionExtensions"/>
/// and validated at startup (RECEIPTS-638). Values are policy that may legitimately differ
/// per environment, so they live here rather than as <c>const</c>s on the service.
/// </summary>
public sealed class ImageValidationOptions
{
	/// <summary>
	/// Maximum pixel width accepted for receipt-image uploads. Images wider than this are
	/// rejected during validation so we never decode an arbitrarily large bitmap. Default
	/// 10,000 matches the historical hardcoded value.
	/// </summary>
	[Range(1, int.MaxValue)]
	public int MaxPixelWidth { get; set; } = 10_000;

	/// <summary>
	/// Maximum pixel height accepted for receipt-image uploads. Images taller than this
	/// are rejected during validation so we never decode an arbitrarily large bitmap.
	/// Default 10,000 matches the historical hardcoded value.
	/// </summary>
	[Range(1, int.MaxValue)]
	public int MaxPixelHeight { get; set; } = 10_000;
}
