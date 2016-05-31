using StockBuddy.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Client.Shared.Services
{
    //TODO: Denne bliver ikke umiddelbart brugt til noget mere. Skal den ikke bare slettes?

    /// <summary>
    /// Bliver kaldt af et view når det instansieres for at finde den viewmodel der skal bruges som datacontext.
    /// Viewets type bliver sendt som parameter, og viewmodel findes ved at generere det matchende viewmodelnavn
    /// (f.eks. OrderView -> OrderViewModel) og resolve den fra en factory. (Typisk en IOC-container).
    /// 
    /// OBS: Bliver ikke kaldt ved views der kan navigeres til, hvor IViewService sørger for at sætte viewmodel som datacontext osv.
    /// Kan evt. bruges ved views der er en del af et større view. (Views der ikke skal navigeres til).
    /// </summary>
    public static class ViewModelLocator
    {
        private const string ViewSuffix = "View";
        private const string ViewModelSuffix = "ViewModel";

        private static Func<Type, object> _viewModelFactory;
        private static Type[] _viewModelTypes;

        static ViewModelLocator()
        {
            _viewModelTypes = (
                from type in typeof(ViewModelLocator).Assembly.GetTypes()
                where type.Name.EndsWith(ViewModelSuffix)
                select type).ToArray();
        }

        public static void SetViewModelFactory(Func<Type, object> viewModelFactory)
        {
            Guard.AgainstNull(() => viewModelFactory);
            _viewModelFactory = viewModelFactory;
        }

        public static object ResolveFromType(Type viewType)
        {
            Guard.AgainstNull(() => viewType);

            if (_viewModelFactory == null)
                throw new InvalidOperationException("ViewModelFactory must be set.");

            var viewName = viewType.Name;

            if (!viewName.EndsWith(ViewSuffix))
                throw new InvalidOperationException(string.Format("Can't locate viewmodel. View name must have suffix '{0}'", ViewSuffix));

            var viewModelName = viewName.Substring(0, viewName.Length - ViewSuffix.Length) + ViewModelSuffix;
            var viewModelType = _viewModelTypes.SingleOrDefault(p => p.Name == viewModelName);

            if (viewModelType == null)
                throw new InvalidOperationException(string.Format("Can't locate viewmodel. Looking for type: {0}", viewModelName));

            return _viewModelFactory(viewModelType);
        }
    }
}
