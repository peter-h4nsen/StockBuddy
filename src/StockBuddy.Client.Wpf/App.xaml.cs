using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using StockBuddy.Client.Shared.Bootstrapping;
using StockBuddy.Client.Shared.ViewModels;
using StockBuddy.Client.Wpf.Services;
using StockBuddy.Client.Wpf.Views;
using System.Threading.Tasks;
using StockBuddy.Shared.Utilities;
using System.Windows.Navigation;

namespace StockBuddy.Client.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            var dbSettings = GetDbConnectionstring();

            var rootWindow = new HostWindow();
            var viewService = new HostWindowViewService(rootWindow);

            HostViewModel hostViewModel = new AppBootstrapper(
                viewService,
                dbSettings.ConnectionString,
                dbSettings.ProviderName
            ).Run();

            rootWindow.DataContext = hostViewModel;

            viewService.SetGlobalButtons(GetGlobalButtons());
            viewService.NavigateTo(typeof(DepositManagementViewModel));
            rootWindow.Show();

            hostViewModel.OnAppStarted();
        }

        private IEnumerable<GlobalButtonViewModel> GetGlobalButtons()
        {
            var imageUriPrexif = "pack://application:,,,/Resources/Images/";

            yield return new GlobalButtonViewModel
            (
                "PAPIRER",
                imageUriPrexif + "stocks.png",
                new[]
                {
                    typeof(StockManagementViewModel),
                    typeof(StockEditorViewModel),
                    typeof(StockDeletionViewModel),
                    typeof(StockSplitManagementViewModel),
                    typeof(GeneralMeetingManagementViewModel),
                }
            );

            yield return new GlobalButtonViewModel
            (
                "INFO",
                imageUriPrexif + "info.png",
                new[]
                {
                    typeof(InfoPageViewModel)
                }
            );
        }

        private ConnectionStringSettings GetDbConnectionstring()
        {
            var key = "StockBuddy";
            var constring = ConfigurationManager.ConnectionStrings[key];

            if (constring == null)
                throw new ConfigurationErrorsException("Missing connectionstring in config file: " + key);

            return constring;
        }
    }
}
