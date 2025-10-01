using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shuryan.Shared.Configurations;

namespace Shuryan.Shared.Extensions
{
	public static class CorsExtensions
	{
		public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
		{
			var corsSettings = configuration.GetSection("CorsSettings").Get<CorsSettings>()!;
			services.AddCors(options =>
			{
				options.AddPolicy(corsSettings.PolicyName, builder =>
				{
					builder.WithOrigins(corsSettings.AllowedOrigins);
					builder.AllowAnyMethod();
					builder.AllowAnyHeader();
					if (corsSettings.AllowCredentials) builder.AllowCredentials();
				});
			});
			return services;
		}
	}
}
