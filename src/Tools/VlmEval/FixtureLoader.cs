using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace VlmEval;

public sealed class FixtureLoader(ILogger<FixtureLoader> logger)
{
	private static readonly IReadOnlyDictionary<string, string> ContentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		[".jpg"] = "image/jpeg",
		[".jpeg"] = "image/jpeg",
		[".png"] = "image/png",
		[".pdf"] = "application/pdf",
	};

	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
	{
		ReadCommentHandling = JsonCommentHandling.Skip,
		AllowTrailingCommas = true,
	};

	public LoadedFixtures LoadFrom(string directory)
	{
		if (!Directory.Exists(directory))
		{
			return new LoadedFixtures([], []);
		}

		List<Fixture> fixtures = [];
		List<string> orphans = [];

		foreach (string filePath in Directory.EnumerateFiles(directory).OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
		{
			string ext = Path.GetExtension(filePath);
			if (!ContentTypes.TryGetValue(ext, out string? contentType))
			{
				continue;
			}

			string sidecarPath = filePath + ".expected.json";
			if (!File.Exists(sidecarPath))
			{
				orphans.Add(filePath);
				continue;
			}

			ExpectedReceipt? expected;
			try
			{
				string json = File.ReadAllText(sidecarPath);
				expected = JsonSerializer.Deserialize<ExpectedReceipt>(json, JsonOptions);
			}
			catch (JsonException ex)
			{
				logger.LogError(ex, "Malformed sidecar {SidecarPath}: {Message}", sidecarPath, ex.Message);
				continue;
			}

			if (expected is null)
			{
				logger.LogError("Sidecar {SidecarPath} deserialized to null; skipping fixture.", sidecarPath);
				continue;
			}

			fixtures.Add(new Fixture(
				Name: Path.GetFileName(filePath),
				FilePath: filePath,
				ContentType: contentType,
				Expected: expected));
		}

		return new LoadedFixtures(fixtures, orphans);
	}
}
