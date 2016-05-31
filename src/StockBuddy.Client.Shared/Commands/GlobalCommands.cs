using System;
using StockBuddy.Shared.Utilities;
using StockBuddy.Client.Shared.Services.Contracts;

namespace StockBuddy.Client.Shared.Commands
{
    public static class GlobalCommands
    {
        private static IViewService _viewService;

        public static void SetViewService(IViewService viewService)
        {
            Guard.AgainstNull(() => viewService);
            _viewService = viewService;
        }

        // ViewNavigatorCommand:
        // Bruges til at navigere til et andet view direkte fra XAML, så man undgår at skulle lave en
        // command i vm'en blot til dette formål. 
        // Commandparameteren sættes til typen på det view der skal navigeres hen til.

        private static readonly ParameterizedRelayCommand<Type> _viewNavigatorCommand = new ParameterizedRelayCommand<Type>(NavigateToView);

        public static ParameterizedRelayCommand<Type> ViewNavigatorCommand
        {
            get { return _viewNavigatorCommand; }
        }

        private static void NavigateToView(Type viewModelType)
        {
            Guard.AgainstNull(() => viewModelType);

            if (_viewService == null)
                throw new InvalidOperationException("ViewService not defined. Must call 'SetViewService' before use.");

            _viewService.NavigateTo(viewModelType);
        }
    }
}
