using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.API.Extensions;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using System.IdentityModel.Tokens.Jwt;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddScoped<IUserResolver, UserResolver>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();




//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();

//    foreach (var endpoint in endpoints.DataSources.SelectMany(ds => ds.Endpoints))
//    {
//        if (endpoint is RouteEndpoint routeEndpoint)
//        {
//            var pattern = routeEndpoint.RoutePattern.RawText;
//            var methodMetadata = routeEndpoint.Metadata
//                .OfType<HttpMethodMetadata>()
//                .FirstOrDefault();

//            var methods = methodMetadata != null
//                ? string.Join(", ", methodMetadata.HttpMethods)
//                : "N/A";

//            Console.WriteLine($"Mapped endpoint: {methods} {pattern}");
//        }
//        else
//        {
//            Console.WriteLine($"Mapped endpoint (non-route): {endpoint.DisplayName}");
//        }
//    }
//});



app.Run();