using Application.Interfaces.Services;
using Common;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class LocalImageStorageService(IConfiguration configuration) : IImageStorageService
{
	// Allowlist of image extensions that may be written to disk. Compared case-insensitively
	// against a normalized (lowercased) caller-supplied value to defeat trivial casing tricks
	// and reject embedded path separators, NTFS alternate-stream markers, NUL bytes, and
	// parent-directory references that Path.GetExtension can pass through unchanged.
	private static readonly HashSet<string> AllowedExtensions =
		new(StringComparer.Ordinal) { ".jpg", ".jpeg", ".png" };

	internal const string InvalidExtensionMessage =
		"Extension must be one of: .jpg, .jpeg, .png.";

	internal const string InvalidFileNameMessage =
		"File name must be a simple basename with no path separators, parent references, or stream markers.";

	private string StorageRoot =>
		configuration[ConfigurationVariables.ImageStoragePath]
		?? Path.Combine(AppContext.BaseDirectory, "ImageStorage");

	public async Task<string> SaveOriginalAsync(Guid receiptId, byte[] imageBytes, string extension, CancellationToken ct)
	{
		ValidateExtension(extension);

		string directory = Path.Combine(StorageRoot, receiptId.ToString());
		Directory.CreateDirectory(directory);

		// Normalize to lowercase so all on-disk filenames are deterministic regardless of
		// the casing the caller supplied (they passed the allowlist either way).
		string fileName = $"original{extension.ToLowerInvariant()}";
		string filePath = Path.Combine(directory, fileName);

		await File.WriteAllBytesAsync(filePath, imageBytes, ct);

		// Return relative path (receiptId/filename) instead of absolute filesystem path
		return Path.Combine(receiptId.ToString(), fileName);
	}

	public string GetImagePath(Guid receiptId, string fileName)
	{
		ValidateFileName(fileName);
		return Path.Combine(StorageRoot, receiptId.ToString(), fileName);
	}

	public Task DeleteReceiptImagesAsync(Guid receiptId, CancellationToken ct)
	{
		string directory = Path.Combine(StorageRoot, receiptId.ToString());
		if (Directory.Exists(directory))
		{
			Directory.Delete(directory, recursive: true);
		}
		return Task.CompletedTask;
	}

	private static void ValidateExtension(string extension)
	{
		if (string.IsNullOrWhiteSpace(extension))
		{
			throw new ArgumentException(InvalidExtensionMessage, nameof(extension));
		}

		// Reject anything with structure beyond a simple ".ext": separators, NUL bytes,
		// stream markers, parent references. Membership check below also catches these,
		// but an explicit reject yields a clearer failure for fuzzy inputs.
		if (ContainsUnsafeCharacter(extension) || extension.Contains(".."))
		{
			throw new ArgumentException(InvalidExtensionMessage, nameof(extension));
		}

		if (!AllowedExtensions.Contains(extension.ToLowerInvariant()))
		{
			throw new ArgumentException(InvalidExtensionMessage, nameof(extension));
		}
	}

	private static void ValidateFileName(string fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			throw new ArgumentException(InvalidFileNameMessage, nameof(fileName));
		}

		if (ContainsUnsafeCharacter(fileName) || fileName.Contains(".."))
		{
			throw new ArgumentException(InvalidFileNameMessage, nameof(fileName));
		}

		// Path.GetFileName strips any directory prefix; if the result differs from the
		// input the caller smuggled in something more than a basename.
		if (!string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal))
		{
			throw new ArgumentException(InvalidFileNameMessage, nameof(fileName));
		}
	}

	private static bool ContainsUnsafeCharacter(string value)
	{
		// Forward/backslashes (path traversal), colon (NTFS alternate streams / drive letters),
		// and NUL (string-truncation tricks against native APIs). These short-circuit ahead of
		// the allowlist check so the error message is consistent.
		foreach (char c in value)
		{
			if (c == '/' || c == '\\' || c == ':' || c == '\0')
			{
				return true;
			}
		}
		return false;
	}
}
