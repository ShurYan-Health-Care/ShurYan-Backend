using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shuryan.Shared.Configurations;

namespace Shuryan.Shared.Extensions
{
    public static class EmailServicesExtentions
    {
        public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
        {
            var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>()!;

            services.AddFluentEmail(emailSettings.FromEmail)
                .AddSmtpSender(new SmtpClient(emailSettings.SmtpHost)
                {
                    Port = emailSettings.SmtpPort,
                    Credentials = new NetworkCredential(emailSettings.SmtpUsername, emailSettings.SmtpPassword),
                    EnableSsl = emailSettings.EnableSsl
                });

            return services;
        }
    }
}
