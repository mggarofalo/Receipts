namespace Infrastructure.Entities.Core;

public class NormalizedDescriptionSettingsEntity
{
	public Guid Id { get; set; }
	public double AutoAcceptThreshold { get; set; }
	public double PendingReviewThreshold { get; set; }
	public DateTimeOffset UpdatedAt { get; set; }
}
