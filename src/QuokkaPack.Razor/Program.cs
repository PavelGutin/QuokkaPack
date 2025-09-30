using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using QuokkaPack.Razor.Extensions;
using QuokkaPack.Razor.Tools;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.ServerCommon.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(
    builder.Environment,
    builder.Configuration);

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddSession();

var apiBaseUrl = builder.Configuration["DownstreamApi:BaseUrl"];

builder.Services.AddHttpClient("QuokkaApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl!);
});


// Configure container-friendly logging
builder.Host.UseContainerFriendlyLogging("QuokkaPack.Razor");

builder.Services.AddJwtAuthentication(builder.Configuration);

// Add comprehensive health checks and monitoring
builder.Services.AddWebApplicationHealthChecks(builder.Configuration, "QuokkaPack.Razor");
builder.Services.AddContainerMonitoring();
builder.Services.AddHealthCheckLogging();

var app = builder.Build();

app.UseRedirectToSetupIfNeeded(apiBaseUrl);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseSession();

app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapStaticAssets();
app.UseStaticFiles();
app.MapRazorPages()
   .WithStaticAssets();

// Map comprehensive health check endpoints
app.MapDetailedHealthChecks();

app.Run();
