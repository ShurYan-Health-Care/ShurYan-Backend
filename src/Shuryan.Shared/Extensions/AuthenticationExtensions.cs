using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shuryan.Shared.Configurations;

namespace Shuryan.Shared.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind JWT settings from appsettings.json
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
            {
                throw new InvalidOperationException("JWT settings are not properly configured. Please check appsettings.json");
            }

            // Register JwtSettings for dependency injection
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true; // Set to true in production
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = jwtSettings.ValidateIssuer,
                    ValidateAudience = jwtSettings.ValidateAudience,
                    ValidateLifetime = jwtSettings.ValidateLifetime,
                    ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes)
                };

                // Events for logging and custom handling
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication Failed: {context.Exception.Message}");
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"Authentication Challenge: {context.Error}, {context.ErrorDescription}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($"Token Validated Successfully for user: {context.Principal?.Identity?.Name}");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers["Authorization"].FirstOrDefault();
                        Console.WriteLine($"Token Received: {(string.IsNullOrEmpty(token) ? "NONE" : "Present")}");
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Admin only policy
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

                // Doctor only policy
                options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));

                // Patient only policy
                options.AddPolicy("PatientOnly", policy => policy.RequireRole("Patient"));

                // Laboratory only policy
                options.AddPolicy("LaboratoryOnly", policy => policy.RequireRole("Laboratory"));

                // Pharmacy only policy
                options.AddPolicy("PharmacyOnly", policy => policy.RequireRole("Pharmacy"));

                // Verifier only policy
                options.AddPolicy("VerifierOnly", policy => policy.RequireRole("Verifier"));

                // Healthcare provider policy (Doctor, Lab, or Pharmacy)
                options.AddPolicy("HealthcareProvider", policy => policy.RequireRole("Doctor", "Laboratory", "Pharmacy"));

                // Verified doctor policy (requires custom authorization handler)
                options.AddPolicy("VerifiedDoctor", policy => { policy.RequireRole("Doctor"); });

                // Admin or Verifier policy
                options.AddPolicy("AdminOrVerifier", policy => policy.RequireRole("Admin", "Verifier"));
            });

            return services;
        }
    }
}
