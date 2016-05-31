using System;
using System.Collections.Generic;
using System.Linq;
using StockBuddy.Domain.Validation;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Domain.Entities
{
    public sealed class Stock : Entity, IValidatable
    {
        private IList<StockSplit> _stockSplits;

        private Stock()
        {
        }

        public Stock(int id, string name, string symbol, string isin, StockTypes stockType, bool isActive)
        {
            Guard.AgainstNull(() => name, () => symbol, () => isin);

            Id = id;
            Name = name;
            Symbol = symbol;
            Isin = isin;
            StockType = stockType;
            IsActive = isActive;
            Trades = new List<Trade>();
        }

        public string Name { get; private set; }
        public string Symbol { get; private set; }
        public string Isin { get; private set; }
        public StockTypes StockType { get; private set; }
        public bool IsActive { get; private set; }
        public ICollection<Trade> Trades { get; private set; }

        /// <summary>
        /// Returns the last stock this one has been splitted into or itself if no stocksplits exist.
        /// </summary>
        private Stock _splitted;
        public Stock Splitted
        {
            get
            {
                if (_splitted == null)
                {
                    var lastStockSplit = _stockSplits.LastOrDefault();
                    _splitted = lastStockSplit != null ? lastStockSplit.NewStock : this;
                }

                return _splitted;
            }
        }

        public void SetStockSplits(IList<StockSplit> stockSplits)
        {
            Guard.AgainstNull(() => stockSplits);
            _stockSplits = stockSplits;
        }

        public IEnumerable<StockSplit> GetStockSplits(DateTime? toDate = null)
        {
            return _stockSplits.Where(p => toDate == null || p.Date.Date <= toDate.Value.Date);
        }

        public IEnumerable<string> BrokenRules()
        {
            var nameBrokenRules = Name.ValidateText("Navn", 50);
            var symbolBrokenRules = Symbol.ValidateText("Symbol", 30);
            var isinBrokenRules = Isin.ValidateText("Isin", 12, true);

            return nameBrokenRules.Concat(symbolBrokenRules).Concat(isinBrokenRules);
        }
    }
}
