using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace StockBuddy.Shared.Utilities
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            if (fieldInfo == null)
                return value.ToString();

            var attribute =
                fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;

            return attribute == null ?
                value.ToString() :
                attribute.Description;
        }

        public static IDictionary<string, Enum> GetDescriptions(this Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException($"Must be a enumeration type: {nameof(enumType)}");

            return Enum.GetValues(enumType).Cast<Enum>()
                .ToDictionary(p => p.GetDescription());
        }
    }
}
