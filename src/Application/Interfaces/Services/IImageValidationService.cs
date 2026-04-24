namespace Application.Interfaces.Services;

/// <summary>
/// Validates uploaded receipt image bytes. Throws <see cref="InvalidOperationException"/>
/// when the payload is not an accepted format or exceeds the dimension budget.
/// </summary>
public interface IImageValidationService
{
	Task ValidateAsync(byte[] imageBytes, CancellationToken ct);
}
