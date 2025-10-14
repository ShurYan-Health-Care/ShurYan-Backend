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
        /// <summary>
        /// Seeds the database with initial data
        /// Usage in Program.cs:
        /// <code>
        /// if (app.Environment.IsDevelopment())
        /// {
        ///     await app.SeedDatabaseAsync();
        /// }
        /// </code>
        /// </summary>
        public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                await DatabaseSeeder.SeedDatabaseAsync(services);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ An error occurred while seeding the database: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Clears all seeded data from the database
        /// Usage in Program.cs (for testing purposes):
        /// <code>
        /// if (app.Environment.IsDevelopment())
        /// {
        ///     await app.ClearDatabaseAsync();
        /// }
        /// </code>
        /// </summary>
        public static async Task ClearDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                await DatabaseSeeder.ClearDatabaseAsync(services);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ An error occurred while clearing the database: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}