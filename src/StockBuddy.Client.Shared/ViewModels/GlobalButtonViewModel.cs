using System;
using System.Linq;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Client.Shared.ViewModels
{
    public sealed class GlobalButtonViewModel
    {
        public string Caption { get; }
        public string ImageUri { get; }
        public Type[] AttachedViewModels { get; }
        public Type NavigatedViewModel { get; }

        public GlobalButtonViewModel(string caption, string imageUri, Type[] attachedViewModels)
        {
            Guard.AgainstNull(() => caption, () => imageUri, () => attachedViewModels);

            Caption = caption;
            ImageUri = imageUri;
            AttachedViewModels = attachedViewModels;

            // The first viewodel in the array is the one being navigated to when
            // the global button is selected in the GUI.
            NavigatedViewModel = AttachedViewModels.FirstOrDefault();
        }

        public bool HasViewModelAttached(Type viewModel)
        {
            return AttachedViewModels.Contains(viewModel);
        }
    }
}
