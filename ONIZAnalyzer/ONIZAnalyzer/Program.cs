using OhNoItsZombiesAnalyzer.Services;
using ONIZAnalyzer.Common.WebSocket;
using ONIZAnalyzer.Components;
using ONIZAnalyzer.WebSocket;

namespace ONIZAnalyzer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();
            builder.Services.AddSignalR();
            builder.Services.AddControllers();
            builder.Services.AddSingleton<ReplayService>();
            builder.Services.AddHttpClient("WebAPI", (sp, client) =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var httpContext = httpContextAccessor.HttpContext;

                if (httpContext != null)
                {
                    client.BaseAddress = new Uri($"{httpContext.Request.Scheme}://{httpContext.Request.Host}");
                }
                else
                {
                    client.BaseAddress = new Uri("https://localhost");
                }
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();
            app.MapControllers();
            app.MapHub<OnizMassReplayHub>(OnizMassAnalysisConstants.MassReplayAnalysisEndPoint);
            app.MapStaticAssets();

            app.MapGet("/", context =>
            {
                context.Response.Redirect("/home");
                return Task.CompletedTask;
            });

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.Run();
        }
    }
}
