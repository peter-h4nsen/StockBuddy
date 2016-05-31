using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace StockBuddy.Client.Wpf.Behaviors
{
    public static class ScaleToWindowSizeBehavior
    {

        public static Window GetParentWindow(DependencyObject obj)
        {
            return (Window)obj.GetValue(ParentWindowProperty);
        }

        public static void SetParentWindow(DependencyObject obj, Window value)
        {
            obj.SetValue(ParentWindowProperty, value);
        }

        public static readonly DependencyProperty ParentWindowProperty = DependencyProperty.RegisterAttached(
            "ParentWindow", typeof(Window), typeof(ScaleToWindowSizeBehavior),
            new PropertyMetadata(null, OnParentWindowChanged));

        private static void OnParentWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mainElement = (FrameworkElement)d;
            var window = (Window)e.NewValue;

            var scaleTransform = new ScaleTransform
            {
                CenterX = 0,
                CenterY = 0
            };

            var scaleValueBinding = new Binding
            {
                Source = window,
                Path = new PropertyPath(ScaleValueProperty)
            };

            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleXProperty, scaleValueBinding);
            BindingOperations.SetBinding(scaleTransform, ScaleTransform.ScaleYProperty, scaleValueBinding);

            mainElement.LayoutTransform = scaleTransform;
            mainElement.SizeChanged += mainElement_SizeChanged;
        }




        public static double GetScaleValue(DependencyObject obj)
        {
            return (double)obj.GetValue(ScaleValueProperty);
        }

        public static void SetScaleValue(DependencyObject obj, double value)
        {
            obj.SetValue(ScaleValueProperty, value);
        }

        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.RegisterAttached(
            "ScaleValue", typeof(double), typeof(ScaleToWindowSizeBehavior),
            new UIPropertyMetadata(1.0, delegate { }, OnCoerceScaleValue));

        private static object OnCoerceScaleValue(DependencyObject d, object baseValue)
        {
            if (baseValue is double)
            {
                var value = (double)baseValue;

                if (double.IsNaN(value))
                    return 1.0;

                value = Math.Max(0.1, value);
                return value;
            }

            return 1.0;
        }



        static void mainElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var mainElement = (FrameworkElement)sender;
            var window = GetParentWindow(mainElement);
            CalculateScale(window);
        }

        private static void CalculateScale(Window window)
        {
            // TODO: Skal sættes til vinduets startup størrelse.
            var scaleX = window.ActualWidth / 1100;
            var scaleY = window.ActualHeight / 800;
            var value = Math.Min(scaleX, scaleY);

            value = Math.Min(value, 1.0);

            SetScaleValue(window, value);
        }
    }
}
