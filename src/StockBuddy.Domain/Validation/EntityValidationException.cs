using System;

namespace StockBuddy.Domain.Validation
{
    public sealed class EntityValidationException : Exception
    {
        public EntityValidationException(string[] brokenValidationRules)
        {
            BrokenValidationRules = brokenValidationRules;
        }

        public EntityValidationException(string brokenValidationRule)
        {
            BrokenValidationRules = new[] { brokenValidationRule };
        }

        public string[] BrokenValidationRules { get; }
    }
}
