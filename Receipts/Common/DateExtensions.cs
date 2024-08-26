namespace Common;

public static class DateTimeExtensions
{
	public static bool Between(this DateTime value, DateTime min, DateTime max)
	{
		return value >= min && value <= max;
	}

	public static bool Between(this DateOnly value, DateOnly min, DateOnly max)
	{
		return value >= min && value <= max;
	}
}