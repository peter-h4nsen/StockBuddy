using System;
using System.Globalization;
using System.Windows.Data;

namespace StockBuddy.Client.Wpf.Converters
{
    public sealed class InvertBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetBool(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetBool(value);
        }

        private bool GetBool(object value)
        {
            if (!(value is bool))
                throw new InvalidOperationException("Type must be a boolean");

            return !((bool)value);
        }
    }
}
