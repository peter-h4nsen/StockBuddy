using System;
using System.Collections.Generic;
using System.Linq;

namespace StockBuddy.Domain.Entities
{
    public sealed class GeneralMeeting : Entity
    {

        private GeneralMeeting()
        { }

        public GeneralMeeting(int id, DateTime meetingDate, decimal dividendRate, int stockID, Stock stock)
        {
            Id = id;
            MeetingDate = meetingDate;
            DividendRate = dividendRate;
            StockID = stockID;
            Stock = stock;
        }

        public DateTime MeetingDate { get; private set; }
        public decimal DividendRate { get; private set; }
        public int StockID { get; private set; }
        public Stock Stock { get; private set; }
    }
}
