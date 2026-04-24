namespace VlmEval;

public sealed class VlmEvalOptions
{
	public string FixturesPath { get; set; } = "fixtures/vlm-eval";

	public int OllamaTimeoutSeconds { get; set; } = 180;

	public bool FailOnAnyFixtureFailure { get; set; } = true;
}
