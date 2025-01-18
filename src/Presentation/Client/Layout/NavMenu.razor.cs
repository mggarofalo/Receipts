using Client.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Client.Layout;

public partial class NavMenu() : ComponentBase
{
	[Inject] public required IClientStorageManager ClientStorageManager { get; set; }
	private const string NavMenuOpen = "NavMenuOpen";
	private bool navMenuOpen;
	private string? NavMenuCssClass => navMenuOpen ? "collapse" : null;

	protected override async Task OnInitializedAsync()
	{
		if (await ClientStorageManager.ContainsKeyAsync(NavMenuOpen))
		{
			navMenuOpen = await ClientStorageManager.GetItemAsync<bool>(NavMenuOpen);
		}
		else
		{
			navMenuOpen = true;
			await ClientStorageManager.SetItemAsync(NavMenuOpen, navMenuOpen);
		}
	}

	private async Task ToggleNavMenuAsync()
	{
		navMenuOpen = !navMenuOpen;
		await ClientStorageManager.SetItemAsync(NavMenuOpen, navMenuOpen);
	}
}
