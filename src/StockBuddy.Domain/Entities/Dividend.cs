using System;
using System.Collections.Generic;
using System.Linq;

namespace StockBuddy.Domain.Entities
{
    public sealed class Dividend : Entity
    {
        private Dividend()
        { }

        public Dividend(int id, int quantity, int generalMeetingID, int depositID, GeneralMeeting generalMeeting, bool isCreated, bool isDifferent)
        {
            Id = id;
            Quantity = quantity;
            GeneralMeetingID = generalMeetingID;
            DepositID = depositID;
            GeneralMeeting = generalMeeting;
            IsCreated = isCreated;
            IsDifferent = isDifferent;
        }

        public int Quantity { get; private set; }
        public int GeneralMeetingID { get; private set; }
        public int DepositID { get; private set; }
        public GeneralMeeting GeneralMeeting { get; private set; }

        public bool IsCreated { get; }
        public bool IsDifferent { get; }

        public decimal DividendPayment => Quantity * GeneralMeeting.DividendRate;
    }
}
