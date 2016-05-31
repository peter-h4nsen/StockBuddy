using System;
using System.Windows.Data;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Client.Wpf.Converters
{
    /// <summary>
    /// Use this when binding to a enum property and the description of the enum should be shown.
    /// </summary>
    public sealed class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var enumValue = value as Enum;

            if (enumValue == null)
                throw new ArgumentException("Argument must be an Enum");

            return enumValue.GetDescription();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
