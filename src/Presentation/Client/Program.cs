using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using MudBlazor.Services;
using Client.Services;
using Client.Interfaces.Services.Core;
using Client.Interfaces.Services.Aggregates;
using Client.Services.Core;
using Client.Services.Aggregates;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services
	.AddMudServices()
	.AddScoped<SignalRService>()
	.AddTransient<IAccountService, AccountService>()
	.AddTransient<IReceiptItemService, ReceiptItemService>()
	.AddTransient<IReceiptService, ReceiptService>()
	.AddTransient<ITransactionService, TransactionService>()
	.AddTransient<IReceiptWithItemsService, ReceiptWithItemsService>()
	.AddTransient<ITransactionAccountService, TransactionAccountService>()
	.AddTransient<ITripService, TripService>();

await builder.Build().RunAsync();
