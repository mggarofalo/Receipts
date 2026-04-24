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
