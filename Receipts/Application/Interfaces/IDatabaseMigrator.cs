namespace Application.Interfaces;

public interface IDatabaseMigrator
{
	Task MigrateAsync();
}