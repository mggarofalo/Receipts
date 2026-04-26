namespace VlmEval;

public sealed record FixtureResult(
	string FixtureName,
	bool Passed,
	TimeSpan Elapsed,
	IReadOnlyList<FieldDiff> FieldDiffs,
	string? Error);

public enum DiffStatus
{
	Pass,
	Fail,
	NotDeclared,
}

public sealed record FieldDiff(
	string Field,
	DiffStatus Status,
	string? Expected,
	string? Actual,
	string? Detail);

/// <summary>
/// Per-run metadata embedded in structured reports. Captured at run start so the report
/// reflects the configured environment (Ollama URL, fixtures path) at the time of the run.
/// </summary>
public sealed record RunInfo(
	DateTimeOffset StartedAt,
	string OllamaUrl,
	string FixturesPath);
