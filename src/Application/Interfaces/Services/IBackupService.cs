namespace Application.Interfaces.Services;

public interface IBackupService
{
	/// <summary>
	/// Exports all domain data from the database to a portable SQLite file.
	/// Returns the path to the generated SQLite file.
	/// </summary>
	Task<string> ExportToSqliteAsync(CancellationToken cancellationToken = default);
}
