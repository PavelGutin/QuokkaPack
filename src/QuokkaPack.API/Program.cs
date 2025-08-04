using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.API.Extensions;
using QuokkaPack.API.Services;
using QuokkaPack.Data;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
);

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddScoped<IUserResolver, UserResolver>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine($"QuokkaPack.API running in environment: {app.Environment.EnvironmentName}");

app.Run();