using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ChadGreen.Management.Client;
using ChadGreen.Management.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
var managementApiBaseUrl = builder.Configuration["ManagementApi:BaseUrl"] ?? "http://localhost:5508";
builder.Services.AddScoped(_ => new ManagementApiHttpClient(new HttpClient { BaseAddress = new Uri(managementApiBaseUrl) }));

await builder.Build().RunAsync();
