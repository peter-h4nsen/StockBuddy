using System;
using System.Collections.Generic;

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
    }
}
