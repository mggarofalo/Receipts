using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace VlmEval.Tests;

/// <summary>
/// Tests for <see cref="FixtureLoader"/>. These exercise the on-disk pairing of fixture files with
/// their <c>.expected.json</c> sidecars, including malformed/orphan/empty cases.
///
/// <para>
/// Each test uses an isolated temporary directory and disposes it via the
/// <see cref="IDisposable"/> implementation on the test class — xUnit creates one instance per
/// test, so the directory is unique per test.
/// </para>
/// </summary>
public class FixtureLoaderTests : IDisposable
{
	private readonly string _tempDir;
	private readonly FixtureLoader _loader;

	public FixtureLoaderTests()
	{
		_tempDir = Path.Combine(Path.GetTempPath(), "vlmeval-tests-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_tempDir);
		_loader = new FixtureLoader(NullLogger<FixtureLoader>.Instance);
	}

	public void Dispose()
	{
		try
		{
			if (Directory.Exists(_tempDir))
			{
				Directory.Delete(_tempDir, recursive: true);
			}
		}
		catch
		{
			// Best-effort cleanup. Ignore if a file is locked on Windows.
		}
		GC.SuppressFinalize(this);
	}

	[Fact]
	public void LoadFrom_NonexistentDirectory_ReturnsEmpty()
	{
		string missing = Path.Combine(_tempDir, "does-not-exist");

		LoadedFixtures result = _loader.LoadFrom(missing);

		result.Fixtures.Should().BeEmpty();
		result.OrphanFiles.Should().BeEmpty();
	}

	[Fact]
	public void LoadFrom_EmptyDirectory_ReturnsEmpty()
	{
		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		result.Fixtures.Should().BeEmpty();
		result.OrphanFiles.Should().BeEmpty();
	}

	[Fact]
	public void LoadFrom_ImageWithoutSidecar_ReportsAsOrphan()
	{
		string imagePath = Path.Combine(_tempDir, "orphan.jpg");
		File.WriteAllBytes(imagePath, [0xFF, 0xD8, 0xFF]); // JPEG magic

		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		result.Fixtures.Should().BeEmpty();
		result.OrphanFiles.Should().ContainSingle().Which.Should().Be(imagePath);
	}

	[Fact]
	public void LoadFrom_PdfWithoutSidecar_ReportsAsOrphan()
	{
		string pdfPath = Path.Combine(_tempDir, "orphan.pdf");
		File.WriteAllBytes(pdfPath, [0x25, 0x50, 0x44, 0x46]); // %PDF magic

		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		result.Fixtures.Should().BeEmpty();
		result.OrphanFiles.Should().ContainSingle().Which.Should().Be(pdfPath);
	}

	[Fact]
	public void LoadFrom_MalformedSidecarJson_SkipsFixture()
	{
		string imagePath = Path.Combine(_tempDir, "bad.jpg");
		string sidecarPath = imagePath + ".expected.json";
		File.WriteAllBytes(imagePath, [0xFF, 0xD8, 0xFF]);
		File.WriteAllText(sidecarPath, "{ this is not valid json", Encoding.UTF8);

		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		// Malformed sidecars are skipped (logged as error). They are NOT reported as orphans.
		result.Fixtures.Should().BeEmpty();
		result.OrphanFiles.Should().BeEmpty();
	}

	[Fact]
	public void LoadFrom_NullDeserializedSidecar_SkipsFixture()
	{
		// JSON literal `null` deserializes to a null ExpectedReceipt → fixture skipped.
		string imagePath = Path.Combine(_tempDir, "nullside.jpg");
		string sidecarPath = imagePath + ".expected.json";
		File.WriteAllBytes(imagePath, [0xFF, 0xD8, 0xFF]);
		File.WriteAllText(sidecarPath, "null", Encoding.UTF8);

		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		result.Fixtures.Should().BeEmpty();
	}

	[Fact]
	public void LoadFrom_ValidPair_LoadsFixture()
	{
		string imagePath = Path.Combine(_tempDir, "valid.jpg");
		string sidecarPath = imagePath + ".expected.json";
		File.WriteAllBytes(imagePath, [0xFF, 0xD8, 0xFF]);
		File.WriteAllText(sidecarPath, """
			{
				"store": "Walmart",
				"total": 70.43
			}
			""", Encoding.UTF8);

		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		result.Fixtures.Should().ContainSingle();
		Fixture fixture = result.Fixtures[0];
		fixture.Name.Should().Be("valid.jpg");
		fixture.FilePath.Should().Be(imagePath);
		fixture.ContentType.Should().Be("image/jpeg");
		fixture.Expected.Store.Should().Be("Walmart");
		fixture.Expected.Total.Should().Be(70.43m);
	}

	[Theory]
	[InlineData(".jpg", "image/jpeg")]
	[InlineData(".jpeg", "image/jpeg")]
	[InlineData(".png", "image/png")]
	[InlineData(".pdf", "application/pdf")]
	public void LoadFrom_KnownExtensions_AreMappedToContentType(string ext, string expectedContentType)
	{
		string imagePath = Path.Combine(_tempDir, "fix" + ext);
		string sidecarPath = imagePath + ".expected.json";
		File.WriteAllBytes(imagePath, [0x00]);
		File.WriteAllText(sidecarPath, "{}", Encoding.UTF8);

		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		result.Fixtures.Should().ContainSingle();
		result.Fixtures[0].ContentType.Should().Be(expectedContentType);
	}

	[Fact]
	public void LoadFrom_UpperCaseExtension_IsMappedToContentType()
	{
		// Extension lookup is OrdinalIgnoreCase.
		string imagePath = Path.Combine(_tempDir, "upper.JPG");
		string sidecarPath = imagePath + ".expected.json";
		File.WriteAllBytes(imagePath, [0xFF, 0xD8, 0xFF]);
		File.WriteAllText(sidecarPath, "{}", Encoding.UTF8);

		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		// On case-insensitive filesystems (Windows) the path may differ in case after enumeration.
		result.Fixtures.Should().ContainSingle();
		result.Fixtures[0].ContentType.Should().Be("image/jpeg");
	}

	[Fact]
	public void LoadFrom_UnknownExtension_IsIgnored()
	{
		// .txt is not in the extension map → silently skipped (not an orphan).
		string textPath = Path.Combine(_tempDir, "notes.txt");
		File.WriteAllText(textPath, "hello", Encoding.UTF8);

		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		result.Fixtures.Should().BeEmpty();
		result.OrphanFiles.Should().BeEmpty();
	}

	[Fact]
	public void LoadFrom_SidecarWithoutFixture_IsIgnored()
	{
		// A bare *.expected.json with no matching image/pdf is silently ignored.
		string sidecarPath = Path.Combine(_tempDir, "ghost.jpg.expected.json");
		File.WriteAllText(sidecarPath, "{}", Encoding.UTF8);

		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		result.Fixtures.Should().BeEmpty();
		result.OrphanFiles.Should().BeEmpty();
	}

	[Fact]
	public void LoadFrom_SidecarWithJsonComments_LoadsSuccessfully()
	{
		// JsonCommentHandling.Skip → // and /* ... */ comments are tolerated.
		string imagePath = Path.Combine(_tempDir, "withcomments.jpg");
		string sidecarPath = imagePath + ".expected.json";
		File.WriteAllBytes(imagePath, [0xFF, 0xD8, 0xFF]);
		File.WriteAllText(sidecarPath, """
			{
				// line comment
				"store": "Target",
				/* block comment */
				"total": 1.23
			}
			""", Encoding.UTF8);

		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		result.Fixtures.Should().ContainSingle();
		result.Fixtures[0].Expected.Store.Should().Be("Target");
		result.Fixtures[0].Expected.Total.Should().Be(1.23m);
	}

	[Fact]
	public void LoadFrom_SidecarWithTrailingComma_LoadsSuccessfully()
	{
		// AllowTrailingCommas = true → trailing commas in objects/arrays are tolerated.
		string imagePath = Path.Combine(_tempDir, "trailing.jpg");
		string sidecarPath = imagePath + ".expected.json";
		File.WriteAllBytes(imagePath, [0xFF, 0xD8, 0xFF]);
		File.WriteAllText(sidecarPath, """
			{
				"store": "Costco",
				"total": 42.10,
			}
			""", Encoding.UTF8);

		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		result.Fixtures.Should().ContainSingle();
		result.Fixtures[0].Expected.Store.Should().Be("Costco");
	}

	[Fact]
	public void LoadFrom_SidecarWithUtf8Bom_LoadsSuccessfully()
	{
		// System.Text.Json's parser handles UTF-8 BOMs natively when reading via byte array,
		// and File.ReadAllText decodes the BOM-prefixed bytes into a BOM-less string.
		// This test pins the current behavior: UTF-8 BOM is accepted.
		string imagePath = Path.Combine(_tempDir, "bom.jpg");
		string sidecarPath = imagePath + ".expected.json";
		File.WriteAllBytes(imagePath, [0xFF, 0xD8, 0xFF]);

		string json = """{"store":"BOM Store"}""";
		byte[] bom = [0xEF, 0xBB, 0xBF];
		byte[] body = Encoding.UTF8.GetBytes(json);
		byte[] withBom = [.. bom, .. body];
		File.WriteAllBytes(sidecarPath, withBom);

		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		result.Fixtures.Should().ContainSingle();
		result.Fixtures[0].Expected.Store.Should().Be("BOM Store");
	}

	[Fact]
	public void LoadFrom_OrdersFixturesAlphabetically()
	{
		string a = Path.Combine(_tempDir, "a.jpg");
		string b = Path.Combine(_tempDir, "b.jpg");
		string c = Path.Combine(_tempDir, "c.jpg");
		foreach (string p in new[] { c, a, b }) // intentionally out of order
		{
			File.WriteAllBytes(p, [0xFF, 0xD8, 0xFF]);
			File.WriteAllText(p + ".expected.json", "{}", Encoding.UTF8);
		}

		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		result.Fixtures.Should().HaveCount(3);
		result.Fixtures[0].Name.Should().Be("a.jpg");
		result.Fixtures[1].Name.Should().Be("b.jpg");
		result.Fixtures[2].Name.Should().Be("c.jpg");
	}

	[Fact]
	public void LoadFrom_MixedOrphansAndValid_ReportsBoth()
	{
		string orphan = Path.Combine(_tempDir, "orphan.jpg");
		string valid = Path.Combine(_tempDir, "valid.jpg");
		File.WriteAllBytes(orphan, [0xFF, 0xD8, 0xFF]);
		File.WriteAllBytes(valid, [0xFF, 0xD8, 0xFF]);
		File.WriteAllText(valid + ".expected.json", "{}", Encoding.UTF8);

		LoadedFixtures result = _loader.LoadFrom(_tempDir);

		result.Fixtures.Should().ContainSingle().Which.Name.Should().Be("valid.jpg");
		result.OrphanFiles.Should().ContainSingle().Which.Should().Be(orphan);
	}
}
