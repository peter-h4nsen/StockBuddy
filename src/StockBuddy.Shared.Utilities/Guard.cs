using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Shared.Utilities
{
    public static class Guard
    {
        /// <summary>
        /// Used to check arguments for null values.
        /// Throws ArgumentNullException if any argument is null.
        /// Arguments should be passed via a lambda: "() => theArg"
        /// </summary>
        //[Obsolete("Maybe this can be replaced by using C# 6 nameof expression?")]
        public static void AgainstNull(params Expression<Func<object>>[] args)
        {
            if (args == null || args.Length == 0)
                return;

            foreach (var arg in args)
            {
                var memberExpression = arg.Body as MemberExpression;

                if (memberExpression == null)
                    throw new ArgumentException("Argument 'args' must contain member expressions");

                // Find the name of the member.
                // When the argument is passed as a closure, a display-class
                // with a field with the right name should be created.
                var name = memberExpression.Member.Name;

                // Compile expression to a delegate.
                var func = arg.Compile();

                // Call the delegate and check if it returns null.
                if (func() == null)
                    throw new ArgumentNullException(name);
            }
        }
    }
}
