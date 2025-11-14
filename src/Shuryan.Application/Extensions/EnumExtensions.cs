using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Shuryan.Application.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            if (value == null)
                return string.Empty;

            var field = value.GetType().GetField(value.ToString());
            
            if (field == null)
                return value.ToString();

            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            
            return attribute?.Description ?? value.ToString();
        }
    }
}
