using Application.Interfaces.Services;
using Common;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class LocalImageStorageService(IConfiguration configuration) : IImageStorageService
{
	private string StorageRoot =>
		configuration[ConfigurationVariables.ImageStoragePath]
		?? Path.Combine(AppContext.BaseDirectory, "ImageStorage");

	public async Task<string> SaveOriginalAsync(Guid receiptId, byte[] imageBytes, string extension, CancellationToken ct)
	{
		string directory = Path.Combine(StorageRoot, receiptId.ToString());
		Directory.CreateDirectory(directory);

		string fileName = $"original{extension}";
		string filePath = Path.Combine(directory, fileName);

		await File.WriteAllBytesAsync(filePath, imageBytes, ct);

		return filePath;
	}

	public async Task<string> SaveProcessedAsync(Guid receiptId, byte[] processedBytes, CancellationToken ct)
	{
		string directory = Path.Combine(StorageRoot, receiptId.ToString());
		Directory.CreateDirectory(directory);

		string filePath = Path.Combine(directory, "processed.png");

		await File.WriteAllBytesAsync(filePath, processedBytes, ct);

		return filePath;
	}

	public string GetImagePath(Guid receiptId, string fileName)
	{
		return Path.Combine(StorageRoot, receiptId.ToString(), fileName);
	}
}
