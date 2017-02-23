using System;
using System.Collections.Generic;
using System.Linq;

namespace StockBuddy.Shared.Utilities
{
    public static class SequenceExtensions
    {
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static void Refill<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            collection.Clear();

            foreach (var item in items)
                collection.Add(item);
        }

        public static bool AllSame<TItem, TComparer>(
                this IEnumerable<TItem> items, Func<TItem, TComparer> selectComparer)
        {
            TComparer comparer = selectComparer(items.First());
            bool result = items.All(p => selectComparer(p).Equals(comparer));
            return result;
        }
    }
}
