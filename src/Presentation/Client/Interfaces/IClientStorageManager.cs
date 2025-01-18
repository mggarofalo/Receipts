namespace Client.Interfaces;

public interface IClientStorageManager
{
	Task SetItemAsync<T>(string key, T value);
	Task<T?> GetItemAsync<T>(string key);
	Task<bool> ContainsKeyAsync(string key);
	Task RemoveItemAsync(string key);
	Task ClearAsync();
}