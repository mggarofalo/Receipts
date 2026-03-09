namespace API.Configuration;

public sealed class RateLimitingOptions
{
	public const string SectionName = "RateLimiting";

	public RateLimitPolicyOptions Global { get; set; } = new() { PermitLimit = 100, WindowMinutes = 1, SegmentsPerWindow = 4 };
	public RateLimitPolicyOptions Auth { get; set; } = new() { PermitLimit = 5, WindowMinutes = 1 };
	public RateLimitPolicyOptions AuthSensitive { get; set; } = new() { PermitLimit = 10, WindowMinutes = 1 };
	public RateLimitPolicyOptions ApiKey { get; set; } = new() { PermitLimit = 10, WindowMinutes = 1 };
}

public sealed class RateLimitPolicyOptions
{
	public int PermitLimit { get; set; }
	public int WindowMinutes { get; set; } = 1;
	public int SegmentsPerWindow { get; set; } = 4;
}
