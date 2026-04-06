namespace Infrastructure.Ynab;

public class YnabRateLimitExceededException : Exception
{
	public int RemainingRequests { get; }
	public int MaxRequests { get; }

	public YnabRateLimitExceededException(int remainingRequests, int maxRequests)
		: base($"YNAB API rate limit would be exceeded. {remainingRequests}/{maxRequests} requests remaining in the current window.")
	{
		RemainingRequests = remainingRequests;
		MaxRequests = maxRequests;
	}

	public YnabRateLimitExceededException(string message) : base(message)
	{
	}

	public YnabRateLimitExceededException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
