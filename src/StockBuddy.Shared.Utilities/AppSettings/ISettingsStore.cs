using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Shared.Utilities.AppSettings
{
    public interface ISettingsStore
    {
        TSettings Load<TSettings>();

        void Save<TSettings>(TSettings appSettings);

        bool IsStoreCreated();

        void CreateStore<TSettings>(TSettings settings);
    }
}
