using Blazored.LocalStorage;
using Client.Interfaces;

namespace Client.Services;

public class ClientStorageManager(ILocalStorageService localStorageService) : IClientStorageManager
{
	public async Task SetItemAsync<T>(string key, T value)
	{
		await localStorageService.SetItemAsync(key, value);
	}

	public async Task<T?> GetItemAsync<T>(string key)
	{
		return await localStorageService.GetItemAsync<T>(key);
	}

	public async Task<bool> ContainsKeyAsync(string key)
	{
		return await localStorageService.ContainKeyAsync(key);
	}

	public async Task RemoveItemAsync(string key)
	{
		await localStorageService.RemoveItemAsync(key);
	}

	public async Task ClearAsync()
	{
		await localStorageService.ClearAsync();
	}
}
