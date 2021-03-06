﻿using System;
using System.Collections.Generic;
using System.Linq;
using StockBuddy.Domain.Validation;

namespace StockBuddy.Domain.Entities
{
    public sealed class Trade : Entity, IValidatable
    {
        private Trade()
        {
        }

        public Trade(int id, bool isBuy, int quantity, decimal price, decimal commission, DateTime tradeDate, 
            int depositID, int stockID, Deposit deposit, Stock stock)
        {
            ID = id;
            IsBuy = isBuy;
            Quantity = quantity;
            Price = price;
            Commission = commission;
            TradeDate = tradeDate;
            DepositID = depositID;
            StockID = stockID;
            Deposit = deposit;
            Stock = stock;
        }

        public bool IsBuy { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }
        public decimal Commission { get; private set; }
        public DateTime TradeDate { get; private set; }
        public int DepositID { get; private set; }
        public int StockID { get; private set; }

        public Deposit Deposit { get; private set; }
        public Stock Stock { get; private set; }

        public int QuantitySigned
        {
            get { return IsBuy ? Quantity : -Quantity; }
        }

        public decimal CommissionSigned
        {
            get { return IsBuy ? Commission : -Commission; }
        }

        public int QuantitySignedSplitted(DateTime? toDate = null) => CalculateSplittedQuantity(QuantitySigned, toDate);
        public int QuantitySplitted(DateTime? toDate = null) => CalculateSplittedQuantity(Quantity, toDate);
        
        private int CalculateSplittedQuantity(int quantity, DateTime? toDate = null)
        {
            return Stock.GetStockSplits(toDate)
                .Aggregate(quantity, (val, s) => (int)Math.Ceiling(val * s.Multiplier));
        }

        public decimal MarketvalueInclCommission => (Quantity * Price) + CommissionSigned;
        
        public IEnumerable<string> BrokenRules()
        {
            return
                ValidationExtensions.ValidateNumber(Quantity, "Antal", 1).Concat(
                Price.ValidateNumber("Pris", 0)).Concat(
                Commission.ValidateNumber("Omkostninger", 0));
        }
    }
}
