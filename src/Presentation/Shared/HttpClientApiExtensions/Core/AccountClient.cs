using Shared.ViewModels.Core;
using System.Net.Http.Json;

namespace Shared.HttpClientApiExtensions.Core;

/// <summary>
/// Provides static methods for interacting with account-related API endpoints.
/// </summary>
public static class AccountClient
{
	/// <summary>
	/// Creates multiple accounts asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the request.</param>
	/// <param name="models">A list of AccountVM objects representing the accounts to create.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains a list of created AccountVM objects, or null if the operation fails.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<List<AccountVM>?> CreateAccountsAsync(this HttpClient httpClient, List<AccountVM> models, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("accounts", models, cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<AccountVM>>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Retrieves a specific account by its ID asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the request.</param>
	/// <param name="id">The unique identifier of the account to retrieve.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the retrieved AccountVM object, or null if not found.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<AccountVM?> GetAccountByIdAsync(this HttpClient httpClient, Guid id, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync($"accounts/{id}", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<AccountVM>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Retrieves all accounts asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the request.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains a list of all AccountVM objects, or null if the operation fails.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<List<AccountVM>?> GetAllAccountsAsync(this HttpClient httpClient, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync("accounts", cancellationToken);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<List<AccountVM>>(cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Updates multiple accounts asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the request.</param>
	/// <param name="models">A list of AccountVM objects representing the updated accounts.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result is true if the update was successful.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<bool> UpdateAccountsAsync(this HttpClient httpClient, List<AccountVM> models, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PutAsJsonAsync("accounts", models, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}

	/// <summary>
	/// Deletes multiple accounts asynchronously.
	/// </summary>
	/// <param name="httpClient">The HttpClient instance to use for the request.</param>
	/// <param name="ids">A list of unique identifiers of the accounts to delete.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result is true if the deletion was successful.</returns>
	/// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
	public static async Task<bool> DeleteAccountsAsync(this HttpClient httpClient, List<Guid> ids, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("accounts/delete", ids, cancellationToken);
		response.EnsureSuccessStatusCode();
		return true;
	}
}