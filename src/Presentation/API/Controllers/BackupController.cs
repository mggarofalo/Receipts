using Application.Interfaces.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/backup")]
[Authorize(Policy = "RequireAdmin")]
public class BackupController(
	IBackupService backupService,
	ILogger<BackupController> logger) : ControllerBase
{
	[HttpPost("export")]
	[EndpointSummary("Export database to a portable SQLite file")]
	[EndpointDescription("Generates a SQLite database containing all domain data (accounts, categories, receipts, items, transactions, adjustments, item templates). Admin-only. Soft-deleted records are excluded.")]
	public async Task<Results<FileStreamHttpResult, StatusCodeHttpResult>> Export(CancellationToken cancellationToken)
	{
		string? filePath = null;
		try
		{
			filePath = await backupService.ExportToSqliteAsync(cancellationToken);

			FileStream stream;
			try
			{
				stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.None,
					bufferSize: 81920, FileOptions.DeleteOnClose | FileOptions.SequentialScan);
			}
			catch
			{
				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
				}

				throw;
			}

			string fileName = $"receipts-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.db";
			return TypedResults.Stream(stream, "application/octet-stream", fileName);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogError(ex, "Failed to export database backup");
			return TypedResults.StatusCode(StatusCodes.Status500InternalServerError);
		}
	}
}
