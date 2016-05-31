using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace StockBuddy.Client.Wpf.Behaviors
{
    public static class FocusElementBehavior
    {
        public static readonly DependencyProperty GiveKeyboardFocusProperty =
            DependencyProperty.RegisterAttached(
                "GiveKeyboardFocus",
                typeof(bool),
                typeof(FocusElementBehavior),
                new PropertyMetadata(false, null, OnGiveKeyboardFocusCoerceValue));

        public static bool GetGiveKeyboardFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(GiveKeyboardFocusProperty);
        }

        public static void SetGiveKeyboardFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(GiveKeyboardFocusProperty, value);
        }

        // Use coerce callback instead of propertychanged callback so method is called even when
        // the value has not changed. (Boolean is set to true each time and never to false).
        private static object OnGiveKeyboardFocusCoerceValue(DependencyObject d, object baseValue)
        {
            var inputElement = d as IInputElement;

            if (inputElement == null)
                return baseValue;

            if ((bool)baseValue)
            {
                // The input element is not necessarily visible yet, so it can't always get focus.
                // Therefore the call is added to the dispatcher queue to delay it until focus can be received.
                d.Dispatcher.InvokeAsync(() =>
                {
                    // The AutoCompleteBox from WPFToolkit does not handle focus properly.
                    // To fix this the focus is set on the internal TextBox found in the template.
                    var autoCompleteBox = inputElement as AutoCompleteBox;

                    if (autoCompleteBox != null)
                        inputElement = (TextBox)autoCompleteBox.Template.FindName("Text", autoCompleteBox);
                    
                    Keyboard.Focus(inputElement);
                }, DispatcherPriority.Input);
            }

            return baseValue;
        }
    }
}
