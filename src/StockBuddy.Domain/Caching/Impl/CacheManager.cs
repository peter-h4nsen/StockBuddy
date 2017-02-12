using StockBuddy.Domain.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Domain.Caching.Impl
{
    // This is not being used atm. Might have to cache data, when doing lots of caluculations etc.
    public class CacheManager
    {
        ConcurrentDictionary<int, Deposit> dic = new ConcurrentDictionary<int, Deposit>();

        public CacheManager()
        {
            
        }

        public void Init(IEnumerable<Deposit> items)
        {
            dic.Clear();

            foreach (var item in items)
            {
                dic.TryAdd(item.Id, item);
            }
        }

        public Deposit Get(int id)
        {
            Deposit deposit;

            if (!dic.TryGetValue(id, out deposit))
                throw new InvalidOperationException($"Item not found in cache. ID: {id}");

            return deposit;
        }

        public void Add(Deposit deposit)
        {
            if (!dic.TryAdd(deposit.Id, deposit))
                throw new InvalidOperationException($"Item already exists in cache. ID: {deposit.Id}");
        }

        public void Remove(int id)
        {
            ((IDictionary<int, Deposit>)dic).Remove(id);
        }
    }
}
