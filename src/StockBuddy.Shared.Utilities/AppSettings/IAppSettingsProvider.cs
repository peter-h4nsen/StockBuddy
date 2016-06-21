using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Shared.Utilities.AppSettings
{
    public interface IAppSettingsProvider<TSettings>
    {
        TSettings Instance { get; }

        void Reload();

        void Save();
    }
}
