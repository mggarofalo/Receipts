namespace VlmEval;

public sealed class VlmEvalOptions
{
	public string FixturesPath { get; set; } = "fixtures/vlm-eval";

	public int OllamaTimeoutSeconds { get; set; } = 180;

	public bool FailOnAnyFixtureFailure { get; set; } = true;

	/// <summary>
	/// Default tolerance applied when comparing money fields (subtotal, total, tax line amounts,
	/// item totalPrice). The check is inclusive: <c>|expected - actual| &lt;= MoneyTolerance</c>
	/// passes. A fixture sidecar may override this via <c>"moneyTolerance": 0.05</c>.
	/// </summary>
	public decimal MoneyTolerance { get; set; } = 0.01m;

	/// <summary>
	/// When set, a structured report of the run is written to this file path. The format is
	/// determined by <see cref="OutputFormat"/>.
	/// </summary>
	public string? ReportPath { get; set; }

	/// <summary>
	/// Format used when writing <see cref="ReportPath"/>. Console output to logs always happens
	/// regardless. <c>Console</c> means the file output is skipped.
	/// </summary>
	public ReportOutputFormat OutputFormat { get; set; } = ReportOutputFormat.Console;
}

public enum ReportOutputFormat
{
	Console,
	Json,
	Markdown,
}
