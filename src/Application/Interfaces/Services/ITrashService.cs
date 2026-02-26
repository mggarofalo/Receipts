namespace Application.Interfaces.Services;

public interface ITrashService
{
	Task PurgeAllDeletedAsync(CancellationToken cancellationToken);
}
