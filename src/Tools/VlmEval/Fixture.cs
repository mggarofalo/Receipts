namespace VlmEval;

public sealed record Fixture(
	string Name,
	string FilePath,
	string ContentType,
	ExpectedReceipt Expected);

public sealed record LoadedFixtures(
	IReadOnlyList<Fixture> Fixtures,
	IReadOnlyList<string> OrphanFiles);
