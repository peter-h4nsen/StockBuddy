using System;
using System.Collections.Generic;
using System.Linq;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Domain.Validation
{
    public static class ValidationExtensions
    {
        public static void Validate(this IValidatable validatable)
        {
            Guard.AgainstNull(() => validatable);

            var brokenRules = validatable.BrokenRules()?.ToArray();

            if (brokenRules != null && brokenRules.Any())
                throw new EntityValidationException(brokenRules);
        }

        public static IEnumerable<string> ValidateText(this string text, string field, int length, bool isExactLength = false)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                yield return string.Format("{0} skal være udfyldt", field);
            }
            else
            {
                if (isExactLength)
                {
                    if (text.Length != length)
                        yield return string.Format("{0} skal indeholde nøjagtigt {1} tegn", field, length);
                }
                else
                {
                    if (text.Length > length)
                        yield return string.Format("{0} må maks indeholde {1} tegn", field, length);
                }
            }
        }

        public static IEnumerable<string> ValidateNumber(this decimal number, string field, decimal minValue, decimal? maxValue = null)
        {
            if (number < minValue)
                yield return string.Format("{0} må ikke være lavere end {1}", field, minValue);
            else if (maxValue != null && number > maxValue)
                yield return string.Format("{0} må ikke være højere end {1}", field, maxValue.Value);
        }
    }
}
