using StockBuddy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBuddy.Domain.DTO
{
    public sealed class DepositInfoDTO
    {
        public DepositInfoDTO(Deposit deposit, /*IEnumerable<StockHolding> stockHoldings,*/ IEnumerable<int> sellableStockIds)
        {
            Deposit = deposit;
            //StockHoldings = stockHoldings;
            SellableStockIds = sellableStockIds;
        }

        public Deposit Deposit { get; }
        //public IEnumerable<StockHolding> StockHoldings { get; }
        public IEnumerable<int> SellableStockIds { get; }
    }
}
