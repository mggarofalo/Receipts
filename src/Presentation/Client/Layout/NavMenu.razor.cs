using Client.Interfaces;

namespace Client.Layout;

public partial class NavMenu(IClientStorageManager clientStorageManager)
{
	private const string NavMenuOpen = "NavMenuOpen";
	private bool navMenuOpen;
	private string? NavMenuCssClass => navMenuOpen ? "collapse" : null;

	protected override async Task OnInitializedAsync()
	{
		if (await clientStorageManager.ContainsKeyAsync(NavMenuOpen))
		{
			navMenuOpen = await clientStorageManager.GetItemAsync<bool>(NavMenuOpen);
		}
		else
		{
			navMenuOpen = true;
			await clientStorageManager.SetItemAsync(NavMenuOpen, navMenuOpen);
		}
	}

	private async Task ToggleNavMenuAsync()
	{
		navMenuOpen = !navMenuOpen;
		await clientStorageManager.SetItemAsync(NavMenuOpen, navMenuOpen);
	}
}
