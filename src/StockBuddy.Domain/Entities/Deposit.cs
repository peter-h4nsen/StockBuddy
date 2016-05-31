using System;
using System.Collections.Generic;
using System.Linq;
using StockBuddy.Domain.Validation;
using StockBuddy.Shared.Utilities;

namespace StockBuddy.Domain.Entities
{
    public sealed class Deposit : Entity, IValidatable
    {
        private Deposit()
        {
        }

        public Deposit(int id, string description, string identityNumber, DepositTypes depositType)
        {
            Guard.AgainstNull(() => description, () => identityNumber);

            Id = id;
            Description = description;
            IdentityNumber = identityNumber;
            DepositType = depositType;
            DateCreated = DateTime.Now;
            Trades = new List<Trade>();
            Dividends = new List<Dividend>();
        }

        public string Description { get; private set; }
        public string IdentityNumber { get; private set; }
        public DepositTypes DepositType { get; private set; }
        public DateTime DateCreated { get; private set; }
        public ICollection<Trade> Trades { get; private set; }
        public ICollection<Dividend> Dividends { get; private set; }

        public void AddTrade(Trade trade)
        {
            Trades.Add(trade);
        }

        public void AddDividend(Dividend dividend)
        {
            Dividends.Add(dividend);
        }

        public Dividend GetDividendForGeneralMeeting(int generalMeetingId)
        {
            var dividend = Dividends.SingleOrDefault(p => p.GeneralMeetingID == generalMeetingId);
            return dividend;
        }

        public IEnumerable<string> BrokenRules()
        {
            var descriptionBrokenRules = Description.ValidateText("Beskrivelse", 100);
            var identityNumberBrokenRules = IdentityNumber.ValidateText("Depotnummer", 30);
            return descriptionBrokenRules.Concat(identityNumberBrokenRules);
        }
    }
}
