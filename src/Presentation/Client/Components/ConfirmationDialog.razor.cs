using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Components;

public partial class ConfirmationDialog
{
	[CascadingParameter]
	public required MudDialogInstance MudDialog { get; set; }

	[Parameter]
	public string ContentText { get; set; } = "Are you sure you want to proceed?";

	void Submit()
	{
		MudDialog.Close(DialogResult.Ok(true));
	}

	void Cancel()
	{
		MudDialog.Cancel();
	}
}