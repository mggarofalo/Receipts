using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Blazored.LocalStorage;
using Client;
using Client.Interfaces;
using Client.Interfaces.Services.Aggregates;
using Client.Interfaces.Services.Core;
using Client.Services;
using Client.Services.Aggregates;
using Client.Services.Core;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
	Uri uri = new(builder.HostEnvironment.BaseAddress);

	return new HttpClient
	{
		BaseAddress = uri
	};
});

builder.Services
	.AddMudServices()
	.AddBlazoredLocalStorage()
	.AddScoped<SignalRService>()
	.AddTransient<IClientStorageManager, ClientStorageManager>()
	.AddTransient<IAccountService, AccountService>()
	.AddTransient<IReceiptItemService, ReceiptItemService>()
	.AddTransient<IReceiptService, ReceiptService>()
	.AddTransient<ITransactionService, TransactionService>()
	.AddTransient<IReceiptWithItemsService, ReceiptWithItemsService>()
	.AddTransient<ITransactionAccountService, TransactionAccountService>()
	.AddTransient<ITripService, TripService>();

await builder.Build().RunAsync();
