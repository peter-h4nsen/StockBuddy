using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Client.TestApp.AppSettingsClasses
{
    //public sealed class AppSettingsOld<TSettings> where TSettings : class, new()
    //{
    //    private readonly ISettingsStoreOld _settingsStore;

    //    public AppSettingsOld(ISettingsStoreOld settingsStore)
    //    {
    //        if (settingsStore == null)
    //            throw new ArgumentNullException(nameof(settingsStore));

    //        _settingsStore = settingsStore;

    //        CreateStoreIfMissing();
    //        Reload();
    //    }

    //    private Lazy<TSettings> _lazyInstance;
    //    public TSettings Instance { get { return _lazyInstance.Value; } }

    //    public void Reload()
    //    {
    //        _lazyInstance = new Lazy<TSettings>(
    //            () => _settingsStore.Load<TSettings>());
    //    }

    //    public void Save()
    //    {
    //        _settingsStore.Save(Instance);
    //    }

    //    private void CreateStoreIfMissing()
    //    {
    //        if (!_settingsStore.IsStoreCreated())
    //        {
    //            var settings = new TSettings();
    //            _settingsStore.CreateStore(settings);
    //        }
    //    }
    //}
}
