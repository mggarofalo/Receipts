namespace Application.Interfaces;

public interface IDatabaseMigratorService
{
	Task MigrateAsync();
}
