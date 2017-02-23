using StockBuddy.Domain.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockBuddy.Domain.Entities
{
    public sealed class StockSplit : Entity, IValidatable
    {
        private StockSplit()
        { }

        public StockSplit(int id, DateTime date, int oldStockID, int newStockID, 
            int ratioFrom, int ratioTo, Stock oldStock, Stock newStock)
        {
            ID = id;
            Date = date;
            OldStockID = oldStockID;
            NewStockID = newStockID;
            RatioFrom = ratioFrom;
            RatioTo = ratioTo;
            OldStock = oldStock;
            NewStock = newStock;
        }

        public DateTime Date { get; private set; }
        public int OldStockID { get; private set; }
        public int NewStockID { get; private set; }
        public int RatioFrom { get; private set; }
        public int RatioTo { get; private set; }
        public Stock OldStock { get; private set; }
        public Stock NewStock { get; private set; }

        public decimal Multiplier => (decimal)RatioTo / RatioFrom;

        public IEnumerable<string> BrokenRules()
        {
            Func<decimal, string, IEnumerable<string>> ratioValidator = 
                (value, field) => value.ValidateNumber(field, 1, 10);

            var ratioFromBrokenRules = ratioValidator(RatioFrom, "Forhold (fra)");
            var ratioToBrokenRules = ratioValidator(RatioTo, "Forhold (til)");
            return ratioFromBrokenRules.Concat(ratioToBrokenRules);
        }
    }
}
