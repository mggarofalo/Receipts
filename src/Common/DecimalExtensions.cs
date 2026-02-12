namespace Common;

public static class DecimalExtensions
{
	public static bool Between(this decimal value, decimal min, decimal max)
	{
		return value >= min && value <= max;
	}
}