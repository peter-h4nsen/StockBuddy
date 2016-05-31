using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Shared.Utilities
{
    public static class StringExtensions
    {
        public static string ReplaceFirst(this string input, string find, string replace)
        {
            var index = input.IndexOf(find);

            return index < 0 ?
                input :
                input.Substring(0, index) + replace + input.Substring(index + find.Length);
        }
    }
}
