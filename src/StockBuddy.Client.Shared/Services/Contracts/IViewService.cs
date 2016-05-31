using System;
using System.Collections.Generic;
using StockBuddy.Client.Shared.Commands;
using StockBuddy.Client.Shared.ViewModels;

namespace StockBuddy.Client.Shared.Services.Contracts
{
    public interface IViewService
    {
        void Init(Func<Type, object> viewModelFactory);
        void SetGlobalButtons(IEnumerable<GlobalButtonViewModel> globalButtons);
        void NavigateTo(Type viewType, object parameter = null);
        void NavigateBack();
        void DisplayValidationErrors(string message);

        object CurrentView { get; }
        bool CanCurrentViewNavigateBack { get; }
        RelayCommand NavigateBackCommand { get; }
    }
}
