using Client.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Client.Layout;

public partial class MainLayout
{
	[Inject] public required IClientStorageManager ClientStorageManager { get; set; }
	private const string DrawerOpen = "DrawerOpen";
	private bool drawerOpen;
	private string? DrawerCssClass => drawerOpen ? "collapse" : null;

	protected override async Task OnInitializedAsync()
	{
		if (await ClientStorageManager.ContainsKeyAsync(DrawerOpen))
		{
			drawerOpen = await ClientStorageManager.GetItemAsync<bool>(DrawerOpen);
		}
		else
		{
			drawerOpen = true;
			await ClientStorageManager.SetItemAsync(DrawerOpen, drawerOpen);
		}
	}

	private async Task ToggleDrawerAsync()
	{
		drawerOpen = !drawerOpen;
		await ClientStorageManager.SetItemAsync(DrawerOpen, drawerOpen);
	}
}