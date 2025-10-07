using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace QuokkaPack.ServerCommon.Extensions
{
    public static class JwtAuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!))
                    {
                        KeyId = "quokka-secret"
                    };
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = key,
                        ValidateIssuerSigningKey = true
                    };
                    options.MapInboundClaims = false;

                    // 👇 Pull token from session instead of Authorization header
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // 1. Check Authorization header first
                            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                            {
                                context.Token = authHeader.Substring("Bearer ".Length);
                            }

                            // 2. Fallback to session (for browser-based requests)
                            // Only access session if it's available (configured in the app)
                            if (string.IsNullOrEmpty(context.Token))
                            {
                                try
                                {
                                    var token = context.HttpContext.Session.GetString("JWT");
                                    if (!string.IsNullOrEmpty(token))
                                    {
                                        context.Token = token;
                                    }
                                }
                                catch (InvalidOperationException)
                                {
                                    // Session not configured - skip session token retrieval
                                    // This is expected in test environments
                                }
                            }

                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            var log = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();
                            log?.LogWarning("JWT authentication failed: {Message}", context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var log = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerEvents>>();
                            log?.LogDebug("JWT token validated successfully");
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
