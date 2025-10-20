using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shuryan.Infrastructure.Data;

namespace Shuryan.Shared.Extensions
{
	public static class DatabaseExtensions
	{
		public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
		{
			// Connection String
			var connectionString = configuration.GetConnectionString("DefaultConnection");

			// Validation
			if (string.IsNullOrEmpty(connectionString))
			{
				throw new InvalidOperationException("Connection String 'DefaultConnection' is missing in appsettings.json");
			}

			// Add DbContext
			services.AddDbContext<ShuryanDbContext>(options =>
			{
				options.UseSqlServer(connectionString, sqlOptions =>
				{
					// Migration Assembly
					sqlOptions.MigrationsAssembly("Shuryan.Infrastructure");
					
					// Command Timeout (in seconds) - increase for complex queries
					sqlOptions.CommandTimeout(120); // 2 minutes instead of default 30 seconds
				});
				
				// Enable split query by default to avoid cartesian explosion
				options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
			});

			return services;
		}
	}
}
