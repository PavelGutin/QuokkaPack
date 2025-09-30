using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuokkaPack.API.Services;
using QuokkaPack.API.Utils;
using QuokkaPack.Data;
using QuokkaPack.ServerCommon.Extensions;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7045",         // Blazor dev server (bare metal)
                "http://localhost:7200",          // Blazor in Docker (based on your docker-compose)
                "http://quokkapack.blazor",       // Internal Docker hostname (if you're using it)
                "http://localhost:80",            // Self-host scenario
                "http://localhost")               // Self-host scenario (alternative)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // If you ever use cookies/session auth
    });
});

// Configure database provider based on connection string and environment
builder.Services.AddQuokkaPackDatabase(builder.Configuration);

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SchemaFilter<NonNullableAsRequiredSchemaFilter>();
    c.SupportNonNullableReferenceTypes(); // important for NRT -> required
    c.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "date" });
});

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()); // string enums
});



// Configure container-friendly logging
builder.Host.UseContainerFriendlyLogging("QuokkaPack.API");

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddScoped<IUserResolver, UserResolver>();

// Add database initialization with retry logic
builder.Services.AddDatabaseInitialization();

// Add comprehensive health checks and monitoring
builder.Services.AddApiHealthChecks(builder.Configuration);
builder.Services.AddContainerMonitoring();
builder.Services.AddHealthCheckLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseSerilogRequestLogging();

app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// Map comprehensive health check endpoints
app.MapDetailedHealthChecks();

//Console.WriteLine($"QuokkaPack.API running in environment: {app.Environment.EnvironmentName}");

Log.Information("QuokkaPack.API started successfully.");

app.Run();

