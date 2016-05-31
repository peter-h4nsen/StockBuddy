using System;
using System.Windows;
using System.Windows.Data;

namespace StockBuddy.Client.Wpf.Converters
{
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean source value to a Visibility.
        /// By default 'true' will result in 'Visible' and 'false' in 'Collapsed'.
        /// Pass the value 'false' as the converter parameter to negate this behavior.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool))
                throw new ArgumentException("Source value type must be 'bool'");

            var sourceValue = (bool)value;
            var notNegated = ParseParameter(parameter);
            sourceValue = notNegated ? sourceValue : !sourceValue;

            return sourceValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a Visibility target value to a boolean.
        /// By default 'Visible' will result in 'true' and 'Collapsed' in 'false'.
        /// Pass the value 'false' as the converter parameter to negate this behavior.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Visibility))
                throw new ArgumentException("Target value type must be 'Visibility'");

            var targetValue = (Visibility)value;
            var notNegated = ParseParameter(parameter);
            var isVisibleValue = notNegated ? true : false;

            return targetValue == Visibility.Visible ? isVisibleValue : !isVisibleValue;
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
