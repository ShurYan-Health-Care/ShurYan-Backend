using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shuryan.Application.Services.Auth;
using Shuryan.Application.Services.Email;
using Shuryan.Application.Services.Token;
using Shuryan.Shared.Configurations;

namespace Shuryan.API.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Register all application services
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure settings from appsettings.json
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<OAuthSettings>(configuration.GetSection("OAuthSettings"));

            // Register services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();

            return services;
        }
    }
}
