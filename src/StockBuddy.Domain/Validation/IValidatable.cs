using System;
using System.Collections.Generic;

namespace StockBuddy.Domain.Validation
{
    public interface IValidatable
    {
        IEnumerable<string> BrokenRules();
    }
}
