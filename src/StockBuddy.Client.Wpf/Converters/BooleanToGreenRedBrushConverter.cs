using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace StockBuddy.Client.Wpf.Converters
{
    public sealed class BooleanToGreenRedBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                throw new ArgumentException("Source value type must be 'bool'");

            if (BrushResources == null)
                throw new InvalidOperationException("Must set the BrushResources property before using BooleanToGreenRedBrushConverter");

            var resourceKey = (bool)value ? "ProfitForegroundBrush" : "LossForegroundBrush";
            return (SolidColorBrush)BrushResources[resourceKey];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public ResourceDictionary BrushResources { get; set; }
    }
}
