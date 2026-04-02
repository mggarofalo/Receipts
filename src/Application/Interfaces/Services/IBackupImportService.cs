using Application.Models;

namespace Application.Interfaces.Services;

public interface IBackupImportService
{
	Task<BackupImportResult> ImportFromSqliteAsync(Stream sqliteStream, CancellationToken cancellationToken);
}
