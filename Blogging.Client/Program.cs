using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blogging.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Use the SAME port as your backend server
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5083/")  // 👈 Match this with your server's HTTPS port
});

// Register services
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();
