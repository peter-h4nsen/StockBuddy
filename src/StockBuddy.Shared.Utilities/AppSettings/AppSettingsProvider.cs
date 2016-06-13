using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Shared.Utilities.AppSettings
{
    public sealed class AppSettingsProvider<TSettings> : IAppSettingsProvider<TSettings> where TSettings : class, new()
    {
        private readonly ISettingsStore _settingsStore;

        public AppSettingsProvider(ISettingsStore settingsStore)
        {
            if (settingsStore == null)
                throw new ArgumentNullException(nameof(settingsStore));

            _settingsStore = settingsStore;

            CreateStoreIfMissing();
            Reload();
        }

        private Lazy<TSettings> _lazyInstance;
        public TSettings Instance { get { return _lazyInstance.Value; } }

        public void Reload()
        {
            _lazyInstance = new Lazy<TSettings>(
                () => _settingsStore.Load<TSettings>());
        }

        public void Save()
        {
            _settingsStore.Save(Instance);
        }

        private void CreateStoreIfMissing()
        {
            if (!_settingsStore.IsStoreCreated())
            {
                var settings = new TSettings();
                _settingsStore.CreateStore(settings);
            }
        }
    }
}
