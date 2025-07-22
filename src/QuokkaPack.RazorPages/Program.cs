using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using QuokkaPack.Data;
using QuokkaPack.RazorPages.Tools;
using QuokkaPack.RazorPages.UserLogin;

var builder = WebApplication.CreateBuilder(args);

var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');



//builder.Services.AddAuthorization(options =>
//{
//    options.FallbackPolicy = options.DefaultPolicy;
//});

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        //options.Prompt = "consent";
    })
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddDownstreamApi("DownstreamApi",builder.Configuration.GetSection("DownstreamApi"))
    .AddInMemoryTokenCaches();

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContext") ?? throw new InvalidOperationException("Connection string 'AppDbContext' not found.")));

builder.Services.AddHttpClient();

builder.Services.AddHttpContextAccessor(); // Needed for IHttpContextAccessor
builder.Services.AddScoped<IApiService, ApiService>();

builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("JWT validation failed: " + context.Exception?.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated for: " + context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddScoped<IUserLoginInitializer, UserLoginInitializer>();
builder.Services.AddScoped<IClaimsTransformation, ClaimsTransformer>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/", policy: null);
    options.Conventions.ConfigureFilter(new AuthorizeForScopesAttribute
    {
        ScopeKeySection = "DownstreamApi:Scopes"
    });
})
.AddMicrosoftIdentityUI();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandler = async ctx => {
        var feature = ctx.Features.Get<IExceptionHandlerFeature>();
        if (feature?.Error is MsalUiRequiredException
            or { InnerException: MsalUiRequiredException }
            or { InnerException.InnerException: MsalUiRequiredException })
        {
            ctx.Response.Cookies.Delete($"{CookieAuthenticationDefaults.CookiePrefix}{CookieAuthenticationDefaults.AuthenticationScheme}");
            ctx.Response.Redirect(ctx.Request.GetEncodedPathAndQuery());
        }
    }
});

//app.Use(async (context, next) =>
//{
//    try
//    {
//        await next();
//    }
//    catch (MicrosoftIdentityWebChallengeUserException)
//    {
//        await context.ChallengeAsync(); // triggers auth redirect
//    }
//});

app.MapStaticAssets();
app.MapRazorPages()
    .RequireAuthorization()
    .WithStaticAssets();
app.MapControllers();



app.Run();
