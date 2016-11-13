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
        public DepositInfoDTO(Deposit deposit,  IEnumerable<int> sellableStockIds, IEnumerable<StockPosition> stockPositions)
        {
            Deposit = deposit;
            SellableStockIds = sellableStockIds;
            StockPositions = stockPositions;
        }

        public Deposit Deposit { get; }
        public IEnumerable<int> SellableStockIds { get; }

        public IEnumerable<StockPosition> StockPositions { get; }
    }
}
