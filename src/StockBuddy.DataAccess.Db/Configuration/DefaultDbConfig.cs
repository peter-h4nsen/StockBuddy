using System;
using System.Data.Entity;

namespace StockBuddy.DataAccess.Db.Configuration
{
    /// <summary>
    /// Use this class if you don't want to configure anything about EF in the config-file.
    /// When placed in the same assembly as the DbContext it will be used automatically.
    /// </summary>
    public class DefaultDbConfig : DbConfiguration
    {
        public DefaultDbConfig()
        {
            SetProviderServices("System.Data.SqlClient", System.Data.Entity.SqlServer.SqlProviderServices.Instance);
        }
    }
}
