using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace ONIZAnalyzer.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        var app = builder.Build();
        await app.RunAsync();
    }
}
