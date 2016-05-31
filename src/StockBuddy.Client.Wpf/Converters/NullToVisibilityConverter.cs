using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StockBuddy.Client.Wpf.Converters
{
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var notNegated = ParseParameter(parameter);

            return value != null
                ? (notNegated ? Visibility.Visible : Visibility.Collapsed)
                : (notNegated ? Visibility.Collapsed : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private bool ParseParameter(object parameter)
        {
            var value = true;

            if (parameter != null)
            {
                var parameterString = parameter as string;

                if (parameterString != null)
                {
                    if (!bool.TryParse(parameterString, out value))
                    {
                        value = true;
                    }
                }
            }

            return value;
        }
    }
}
