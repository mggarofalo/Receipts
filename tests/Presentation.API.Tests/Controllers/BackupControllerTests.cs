using API.Controllers;
using Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace Presentation.API.Tests.Controllers;

public class BackupControllerTests : IDisposable
{
	private readonly Mock<IBackupService> _backupServiceMock;
	private readonly Mock<ILogger<BackupController>> _loggerMock;
	private readonly BackupController _controller;
	private readonly List<string> _tempFiles = [];

	public BackupControllerTests()
	{
		_backupServiceMock = new Mock<IBackupService>();
		_loggerMock = ControllerTestHelpers.GetLoggerMock<BackupController>();
		_controller = new BackupController(_backupServiceMock.Object, _loggerMock.Object);
	}

	[Fact]
	public async Task Export_Success_ReturnsFileStream()
	{
		// Arrange
		string tempFile = Path.GetTempFileName();
		_tempFiles.Add(tempFile);
		await File.WriteAllTextAsync(tempFile, "test-sqlite-content");

		_backupServiceMock.Setup(s => s.ExportToSqliteAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(tempFile);

		// Act
		var result = await _controller.Export(CancellationToken.None);

		// Assert
		FileStreamHttpResult fileResult = result.Result.Should().BeOfType<FileStreamHttpResult>().Subject;
		fileResult.ContentType.Should().Be("application/octet-stream");
		fileResult.FileDownloadName.Should().StartWith("receipts-backup-");
		fileResult.FileDownloadName.Should().EndWith(".db");
	}

	[Fact]
	public async Task Export_ServiceThrows_Returns500()
	{
		// Arrange
		_backupServiceMock.Setup(s => s.ExportToSqliteAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException("Database error"));

		// Act
		var result = await _controller.Export(CancellationToken.None);

		// Assert
		result.Result.Should().BeOfType<StatusCodeHttpResult>()
			.Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
	}

	[Fact]
	public async Task Export_CancellationRequested_PropagatesException()
	{
		// Arrange
		_backupServiceMock.Setup(s => s.ExportToSqliteAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new OperationCanceledException());

		// Act
		Func<Task> act = () => _controller.Export(CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<OperationCanceledException>();
	}

	public void Dispose()
	{
		foreach (string path in _tempFiles)
		{
			try
			{
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch
			{
				// Best effort cleanup
			}
		}

		GC.SuppressFinalize(this);
	}
}
