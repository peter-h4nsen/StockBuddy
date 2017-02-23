using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Shared.Utilities
{
    public static class TaskExtensions
    {

        // Call this on a task when it is not being awaited. (Fire and forget).
        // This will remove the following compiler warning:
        // CS4014: "Because this call is not awaited, execution of the current method continues before the call is completed"
        public static void Forget(this Task t)
        { }
    }
}
