using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shuryan.Infrastructure.Seeders;

namespace Shuryan.Shared.Extensions
{
    public static class DatabaseSeederExtension
    {
        public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                await DatabaseSeeder.SeedDatabaseAsync(services);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static async Task ClearDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                await DatabaseSeeder.ClearDatabaseAsync(services);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}