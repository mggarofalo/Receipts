using Microsoft.AspNetCore.Components;

namespace Client.Components;
public partial class DateOnlyPicker
{
	[Parameter]
	public DateOnly Value { get; set; }

	[Parameter]
	public EventCallback<DateOnly> ValueChanged { get; set; }

	[Parameter]
	public string Label { get; set; } = "Select Date";

	private DateTime? Date
	{
		get
		{
			return Value == default ? null : Value.ToDateTime(TimeOnly.MinValue);
		}
		set
		{
			if (value.HasValue)
			{
				DateOnly newValue = DateOnly.FromDateTime(value.Value);
				if (newValue != Value)
				{
					Value = newValue;
					ValueChanged.InvokeAsync(Value);
				}
			}
			else
			{
				Value = default;
				ValueChanged.InvokeAsync(Value);
			}
		}
	}
}