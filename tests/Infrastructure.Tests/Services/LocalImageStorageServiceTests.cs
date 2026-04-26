using Common;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Infrastructure.Tests.Services;

/// <summary>
/// Tests for <see cref="LocalImageStorageService"/>. Covers the extension allowlist on
/// <c>SaveOriginalAsync</c> and basename validation on <c>GetImagePath</c> against
/// path-traversal, NTFS alternate-stream, and embedded-NUL attack patterns described
/// in RECEIPTS-641.
/// </summary>
public sealed class LocalImageStorageServiceTests : IDisposable
{
	private readonly string _tempRoot;
	private readonly LocalImageStorageService _service;

	public LocalImageStorageServiceTests()
	{
		_tempRoot = Path.Combine(Path.GetTempPath(), "receipts-tests-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_tempRoot);

		Mock<IConfiguration> configuration = new();
		// Per project memory: use the flat indexer, not GetSection/GetConnectionString,
		// because Moq does not implement those extension methods.
		configuration.Setup(c => c[ConfigurationVariables.ImageStoragePath]).Returns(_tempRoot);

		_service = new LocalImageStorageService(configuration.Object);
	}

	public void Dispose()
	{
		if (Directory.Exists(_tempRoot))
		{
			Directory.Delete(_tempRoot, recursive: true);
		}
	}

	// ---------------------------------------------------------------------
	// SaveOriginalAsync — happy path
	// ---------------------------------------------------------------------

	[Theory]
	[InlineData(".jpg")]
	[InlineData(".jpeg")]
	[InlineData(".png")]
	[InlineData(".JPG")]
	[InlineData(".JPEG")]
	[InlineData(".PNG")]
	public async Task SaveOriginalAsync_AllowedExtension_WritesFile(string extension)
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		byte[] bytes = [0x01, 0x02, 0x03];

		// Act
		string relativePath = await _service.SaveOriginalAsync(receiptId, bytes, extension, CancellationToken.None);

		// Assert — relative path uses lowercased extension and correct receipt folder
		string expectedRelative = Path.Combine(receiptId.ToString(), $"original{extension.ToLowerInvariant()}");
		relativePath.Should().Be(expectedRelative);

		string absolutePath = Path.Combine(_tempRoot, relativePath);
		File.Exists(absolutePath).Should().BeTrue();
		(await File.ReadAllBytesAsync(absolutePath)).Should().Equal(bytes);
	}

	// ---------------------------------------------------------------------
	// SaveOriginalAsync — attack vectors from RECEIPTS-641
	// ---------------------------------------------------------------------

	[Theory]
	// Path-traversal smuggled through Path.GetExtension (returns everything from the last '.')
	[InlineData(".jpg/../../etc/passwd")]
	[InlineData(".jpg\\..\\..\\Windows\\System32\\evil")]
	// NTFS alternate data streams
	[InlineData(".jpg:malicious")]
	[InlineData(".jpg:$DATA")]
	// Embedded NUL (string truncation trick against unmanaged APIs)
	[InlineData(".jpg\0evil")]
	// Disallowed extensions
	[InlineData(".gif")]
	[InlineData(".exe")]
	[InlineData(".php")]
	[InlineData(".txt")]
	// Missing leading dot
	[InlineData("jpg")]
	[InlineData("png")]
	// Parent reference only
	[InlineData("..")]
	[InlineData(".")]
	public async Task SaveOriginalAsync_RejectsUnsafeExtension(string extension)
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		byte[] bytes = [0x01];

		// Act
		Func<Task> act = () => _service.SaveOriginalAsync(receiptId, bytes, extension, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage($"*{LocalImageStorageService.InvalidExtensionMessage}*");

		// Side-effect check: validation must short-circuit before the receipt directory is
		// even created. Asserting non-existence (rather than emptiness) catches a regression
		// where someone reorders Directory.CreateDirectory above the ValidateExtension call.
		string receiptDir = Path.Combine(_tempRoot, receiptId.ToString());
		Directory.Exists(receiptDir).Should().BeFalse(
			"validation must reject before any directory is created");
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("\t")]
	public async Task SaveOriginalAsync_RejectsEmptyExtension(string extension)
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		byte[] bytes = [0x01];

		// Act
		Func<Task> act = () => _service.SaveOriginalAsync(receiptId, bytes, extension, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ArgumentException>();
	}

	[Fact]
	public async Task SaveOriginalAsync_NullExtension_ThrowsArgumentException()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		byte[] bytes = [0x01];

		// Act
		Func<Task> act = () => _service.SaveOriginalAsync(receiptId, bytes, null!, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<ArgumentException>();
	}

	// ---------------------------------------------------------------------
	// GetImagePath — happy path
	// ---------------------------------------------------------------------

	[Theory]
	[InlineData("original.jpg")]
	[InlineData("original.jpeg")]
	[InlineData("original.png")]
	[InlineData("original.JPG")]
	public void GetImagePath_ValidBasename_ReturnsCombinedPath(string fileName)
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();
		string expected = Path.Combine(_tempRoot, receiptId.ToString(), fileName);

		// Act
		string actual = _service.GetImagePath(receiptId, fileName);

		// Assert
		actual.Should().Be(expected);
	}

	// ---------------------------------------------------------------------
	// GetImagePath — attack vectors
	// ---------------------------------------------------------------------

	[Theory]
	// Path traversal
	[InlineData("../etc/passwd")]
	[InlineData("..\\Windows\\System32\\config\\SAM")]
	[InlineData("foo/bar.jpg")]
	[InlineData("foo\\bar.jpg")]
	[InlineData("..")]
	[InlineData(".")]
	[InlineData("a/../b.jpg")]
	// NTFS alternate streams / drive letters
	[InlineData("original.jpg:malicious")]
	[InlineData("C:original.jpg")]
	// NUL byte truncation
	[InlineData("original.jpg\0evil")]
	// Absolute paths
	[InlineData("/etc/passwd")]
	[InlineData("\\\\server\\share\\file.jpg")]
	public void GetImagePath_RejectsUnsafeFileName(string fileName)
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();

		// Act
		Action act = () => _service.GetImagePath(receiptId, fileName);

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage($"*{LocalImageStorageService.InvalidFileNameMessage}*");
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("\t")]
	public void GetImagePath_RejectsEmptyFileName(string fileName)
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();

		// Act
		Action act = () => _service.GetImagePath(receiptId, fileName);

		// Assert
		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void GetImagePath_NullFileName_ThrowsArgumentException()
	{
		// Arrange
		Guid receiptId = Guid.NewGuid();

		// Act
		Action act = () => _service.GetImagePath(receiptId, null!);

		// Assert
		act.Should().Throw<ArgumentException>();
	}
}
