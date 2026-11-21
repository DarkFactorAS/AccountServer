using AccountWeb.Controllers;
using AccountWeb.Providers;
using DFCommonLib.Config;
using DFCommonLib.Logger;
using DFCommonLib.Utils;

const string Html = "<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>DarkFactor - Accounts</title></head><body><h1 style=\"text-align:center\">DarkFactor - Accounts</h1><p style=\"text-align:center\">Frontend not built. Run <code>npm run build</code> in AccountWeb.</p></body></html>";

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAccountWebAuthentication();
builder.Services.AddSingleton<IConfigurationHelper, ConfigurationHelper<AccountWebSettings>>();
builder.Services.AddAccountServerClient();
builder.Services.AddSingleton<SetupState>();
new DFServices(builder.Services)
    .SetupLogger()
    .LogToConsole(DFLogLevel.INFO);

var app = builder.Build();
// Static files (the SPA shell and its assets) are public; they hold no data. They must run
// before authorization, or the fallback policy would block them for logged-out users.
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

var settings = app.Services.GetRequiredService<IConfigurationHelper>().Settings as AccountWebSettings;
DFLogger.LogOutput(
    DFLogLevel.INFO,
    "Startup",
    $"Environment '{builder.Environment.EnvironmentName}', account server '{settings?.AccountServer?.Endpoint}'.");
StartupReport.Log(settings, builder.Environment.EnvironmentName);

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" })).AllowAnonymous();
AuthEndpoints.Map(app);
AccountEndpoints.Map(app);
SetupEndpoints.Map(app);

// SPA fallback (public: the app shows its login page); unknown /api routes stay 404.
app.MapFallback(context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return Task.CompletedTask;
    }

    context.Response.ContentType = "text/html";
    var index = app.Environment.WebRootFileProvider.GetFileInfo("index.html");
    return index.Exists ? context.Response.SendFileAsync(index) : context.Response.WriteAsync(Html);
}).AllowAnonymous();

app.Run();
