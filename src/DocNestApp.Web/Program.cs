using DocNestApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddServiceDiscovery();

builder.Services.AddHttpClient<DocNestApiClient>(client =>
{
    var baseUrl = builder.Configuration["DocNest:ApiBaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Missing DocNest:ApiBaseUrl (set by AppHost or appsettings).");

    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();