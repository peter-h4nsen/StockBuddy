using System;
using System.Windows;
using StockBuddy.Client.Shared.Services;

namespace StockBuddy.Client.Wpf.Behaviors
{
    //TODO: Bruges ikke? Skal den ikke bare væk??

    /// <summary>
    /// Attached behavior "AutoViewViewModel" sættes på det element der skal have en viewmodel sat som datacontext.
    /// Passende viewmodel findes via "ViewModelLocator" ud fra viewets type. F.eks. MainView finder MainViewModel.
    /// </summary>
    public static class ViewModelBehaviors
    {
        public static bool GetAutoWireViewModel(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoWireViewModelProperty);
        }

        public static void SetAutoWireViewModel(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoWireViewModelProperty, value);
        }

        public static readonly DependencyProperty AutoWireViewModelProperty = DependencyProperty.RegisterAttached(
            "AutoWireViewModel", typeof(bool), typeof(ViewModelBehaviors),
            new PropertyMetadata(false, AutoWireViewModelChanged));

        private static void AutoWireViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as FrameworkElement;

            if (view == null)
                return;

            var viewModel = ViewModelLocator.ResolveFromType(view.GetType());
            view.DataContext = viewModel;
        }
    }
}
