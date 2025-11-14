using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation.AspNetCore;

namespace Shuryan.Application.Extensions
{
    public static class ValidationExtensions
    {
        public static IServiceCollection AddFluentValidation(this IServiceCollection services)
        {
            // Register all validators from this assembly
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Configure validation behavior
            ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Stop;
            // ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

            return services;
        }
    }
}
